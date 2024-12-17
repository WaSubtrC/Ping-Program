using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

class PingProgram
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("请输入需要Ping的内容（输入\"-1\"退出）：");
            string input = Console.ReadLine();

            if (input == "-1")
            {
                break;
            }

            string[] tokens = input.Split(' ');
            string host = tokens[0];
            int count = 4; // 默认Ping次数
            int dataSize = 32; // 默认数据包大小
            bool showHelp = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "-n" && i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out int n))
                {
                    count = n;
                }
                else if (tokens[i] == "datasize" && i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out int size))
                {
                    dataSize = size;
                }
                else if (tokens[i] == "help")
                {
                    showHelp = true;
                }
            }

            if (showHelp)
            {
                ShowHelp();
            }
            else
            {
                PingHost(host, count, dataSize);
            }
        }
    }

    static void PingHost(string host, int count, int dataSize)
    {
        Ping pingSender = new Ping();
        PingOptions options = new PingOptions();
        options.DontFragment = true;

        byte[] buffer = Encoding.ASCII.GetBytes(new string('a', dataSize));
        int timeout = 1000; // 超时时间为1秒
        PingReply reply;

        int successCount = 0;
        long totalTime = 0;
        int lostPackets = 0;

        for (int i = 0; i < count; i++)
        {
            try
            {
                reply = pingSender.Send(host, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    successCount++;
                    totalTime += reply.RoundtripTime;
                    Console.WriteLine($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}");
                }
                else
                {
                    lostPackets++;
                    Console.WriteLine($"Request timed out.");
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"Ping failed: {ex.Message}");
                lostPackets++;
            }
        }

        Console.WriteLine($"\nPing statistics for {host}:");
        Console.WriteLine($"    Packets: Sent = {count}, Received = {successCount}, Lost = {lostPackets} ({(lostPackets * 100) / count}% loss)");
        if (successCount > 0)
        {
            Console.WriteLine($"Approximate round trip times in milli-seconds:");
            Console.WriteLine($"    Minimum = {totalTime / successCount}ms, Maximum = {totalTime / successCount}ms, Average = {totalTime / successCount}ms");
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Ping程序使用说明：");
        Console.WriteLine("    [host] [-n count] [datasize size] [help]");
        Console.WriteLine("选项说明：");
        Console.WriteLine("    host        需要Ping的主机名或IP地址");
        Console.WriteLine("    -n count    指定发送的Ping请求数目");
        Console.WriteLine("    datasize size    指定每个Ping请求的数据包大小");
        Console.WriteLine("    help        显示帮助信息");
    }
}
