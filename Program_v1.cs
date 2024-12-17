using System.Net.NetworkInformation;
using System.Text;

namespace Ping_Program
{
    
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

            var (host, count, dataSize, showHelp) = ParseArguments(input);

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

    static (string host, int count, int dataSize, bool showHelp) ParseArguments(string input)
    {
        string[] tokens = input.Split(' ');
        string host = tokens[0];
        int count = 4; // 默认Ping次数
        int dataSize = 32; // 默认数据包大小
        bool showHelp = false;

        for (int i = 0; i < tokens.Length; i++) 
        {
            if (tokens[i] == "-n" && i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out int n) && n > 0)
            {
                count = n;
                i++; // 跳过下一个参数
            }
            else if (tokens[i] == "datasize" && i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out int size) && size > 0)
            {
                dataSize = size;
                i++; // 跳过下一个参数
            }
            else if (tokens[i] == "help")
            {
                showHelp = true;
            }
        }

        return (host, count, dataSize, showHelp);
    }

    static void PingHost(string host, int count, int dataSize)
    {
        using (var pingSender = new Ping())
        {
            PingOptions options = new PingOptions { DontFragment = true };
            byte[] buffer = Encoding.ASCII.GetBytes(new string('a', dataSize));
            int timeout = 1000; // 超时时间为1秒
            
            long totalTime = 0;
            long minTime = long.MaxValue;
            long maxTime = long.MinValue;
            int successCount = 0;
            int lostPackets = 0;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    PingReply reply = pingSender.Send(host, timeout, buffer, options);
                    if (reply.Status == IPStatus.Success)
                    {
                        successCount++;
                        totalTime += reply.RoundtripTime;
                        minTime = Math.Min(minTime, reply.RoundtripTime);
                        maxTime = Math.Max(maxTime, reply.RoundtripTime);
                        Console.WriteLine($"来自 {reply.Address} 的回复：字节数={reply.Buffer.Length} 时间={reply.RoundtripTime}ms TTL={reply.Options.Ttl}");
                    }
                    else
                    {
                        lostPackets++;
                        Console.WriteLine("请求超时。");
                    }
                }
                catch (PingException ex)
                {
                    Console.WriteLine($"Ping 失败：{ex.Message}");
                    lostPackets++;
                }
                catch (Exception ex) // 捕获其他异常
                {
                    Console.WriteLine($"发生了意外错误：{ex.Message}");
                    lostPackets++;
                }
            }

            Console.WriteLine($"\n{host} 的 Ping 统计信息：");
            Console.WriteLine($"    数据包：发送 = {count}, 接收 = {successCount}, 丢失 = {lostPackets} ({(lostPackets * 100) / count}% 丢包)");
            if (successCount > 0)
            {
                Console.WriteLine($"大致往返时间（毫秒）：");
                Console.WriteLine($"    最小 = {minTime}ms, 最大 = {maxTime}ms, 平均 = {totalTime / successCount}ms");
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Ping程序使用说明：");
        Console.WriteLine("    [host] [-n count] [datasize size] [help]");
        Console.WriteLine("选项说明：");
        Console.WriteLine("    host        需要Ping的主机名或IP地址");
        Console.WriteLine("    -n count    指定发送的Ping请求数目（大于0）");
        Console.WriteLine("    datasize size    指定每个Ping请求的数据包大小（大于0）");
        Console.WriteLine("    help        显示帮助信息");
    }
}
}
