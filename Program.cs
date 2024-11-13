namespace Ping_Program
{


    using System.Net;
    using System.Net.Sockets;

    class PingProgram
    {
        static void Main(string[] args)
        {
            Console.Write("请输入要 Ping 的目标 IP 地址: ");
            string targetIp = Console.ReadLine();

            Console.Write("请输入 Ping 的次数 (默认为 4 次): ");
            int pingCount = 4;
            if (int.TryParse(Console.ReadLine(), out pingCount) && pingCount <= 0)
            {
                pingCount = 4;
            }

            Console.Write("请输入每个 ICMP 包的数据长度 (默认为 64 字节): ");
            int dataSize = 64;
            if (int.TryParse(Console.ReadLine(), out dataSize) && dataSize <= 0)
            {
                dataSize = 64;
            }

            SendPing(targetIp, pingCount, dataSize);
        }

        static void SendPing(string destination, int count, int dataSize)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
            {
                socket.SendTimeout = 2000;
                IPAddress targetAddress = IPAddress.Parse(destination);

                int sent = 0;
                int received = 0;
                int minTime = int.MaxValue;
                int maxTime = 0;
                int totalTime = 0;

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        byte[] sendBuffer = CreateIcmpEchoRequest(i, dataSize);
                        long sendTime = DateTime.Now.Ticks;
                        socket.SendTo(sendBuffer, new IPEndPoint(targetAddress, 0));
                        sent++;

                        byte[] receiveBuffer = new byte[1024];
                        EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0); // 使用 EndPoint 类型
                        int bytesReceived = socket.ReceiveFrom(receiveBuffer, ref receiveEndPoint); // 传递 EndPoint 类型
                        long receiveTime = DateTime.Now.Ticks;
                        int rtt = (int)((receiveTime - sendTime) / 10000.0); // 以毫秒为单位计算往返时间

                        minTime = Math.Min(minTime, rtt);
                        maxTime = Math.Max(maxTime, rtt);
                        totalTime += rtt;
                        received++;

                        Console.WriteLine($"Reply from {targetAddress}: bytes={bytesReceived} time={rtt}ms");
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"Request timed out for sequence number {i}");
                    }
                }

                Console.WriteLine($"\nPing statistics for {destination}:");
                Console.WriteLine(
                    $"    Packets: Sent = {sent}, Received = {received}, Lost = {sent - received} ({(sent - received) * 100.0 / sent:F2}% loss)");
                Console.WriteLine($"    Approximate round trip times in milli-seconds:");
                Console.WriteLine(
                    $"    Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime * 1.0 / received:F2}ms");
            }
        }


        static byte[] CreateIcmpEchoRequest(int sequenceNumber, int dataSize)
        {
            byte[] buffer = new byte[28 + dataSize];
            buffer[0] = 8; // ICMP 类型 (8 = Echo Request)
            buffer[1] = 0; // ICMP 代码 (0 = Echo Request)
            buffer[2] = 0; // ICMP 校验和 (待计算)
            buffer[3] = 0; // ICMP 校验和 (待计算)
            buffer[4] = (byte)(sequenceNumber >> 8);
            buffer[5] = (byte)sequenceNumber;
            buffer[6] = 0; // 标识符 (高 8 位)
            buffer[7] = 0; // 标识符 (低 8 位)

            // 填充数据部分
            for (int i = 28; i < buffer.Length; i++)
            {
                buffer[i] = (byte)'x';
            }

            // 计算 ICMP 校验和
            ushort checksum = CalculateChecksum(buffer);
            buffer[2] = (byte)(checksum >> 8);
            buffer[3] = (byte)checksum;

            return buffer;
        }

        static ushort CalculateChecksum(byte[] buffer)
        {
            uint checksum = 0;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                checksum += (ushort)((buffer[i] << 8) | buffer[i + 1]);
            }

            checksum = (checksum >> 16) + (checksum & 0xFFFF);
            checksum += (checksum >> 16);
            return (ushort)~checksum;
        }
    }
}