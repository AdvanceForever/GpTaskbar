using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using GpTaskbar.Models;

namespace GpTaskbar.Services
{
    public class StockService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://hq.sinajs.cn/list=";

        public StockService()
        {
            // 注册编码提供程序以支持GBK编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://finance.sina.com.cn");
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

            
        }

        public async Task<StockData> GetStockDataAsync(string symbol)
        {
            try
            {
                var url = $"{BaseUrl}{symbol}";
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                // 新浪接口返回GBK编码，使用GBK编码
                var contentBytes = await response.Content.ReadAsByteArrayAsync();
                var responseText = System.Text.Encoding.GetEncoding("GBK").GetString(contentBytes);
                
                return ParseStockData(symbol, responseText);
            }
            catch (Exception ex)
            {
                throw new Exception($"获取股票数据失败: {ex.Message}");
            }
        }

        public async Task<List<StockData>> GetMultipleStocksAsync(List<string> symbols)
        {
            var tasks = new List<Task<StockData>>();
            
            foreach (var symbol in symbols)
            {
                tasks.Add(GetStockDataAsync(symbol));
            }

            var results = await Task.WhenAll(tasks);
            return new List<StockData>(results);
        }

        private StockData ParseStockData(string symbol, string response)
        {
            // 新浪股票接口返回格式示例：var hq_str_sh000001="上证指数,3278.48,3289.78,3267.16,3294.24,3255.55,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2024-03-06,15:30:00,00";
            
            var startIndex = response.IndexOf('"');
            var endIndex = response.LastIndexOf('"');
            
            if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
            {
                throw new Exception("股票数据格式错误");
            }

            var dataStr = response.Substring(startIndex + 1, endIndex - startIndex - 1);
            var dataParts = dataStr.Split(',');

            if (dataParts.Length < 4)
            {
                throw new Exception("股票数据不完整");
            }

            var stockData = new StockData
            {
                Symbol = symbol,
                Name = dataParts[0],
                CurrentPrice = decimal.Parse(dataParts[3]),
                Change = decimal.Parse(dataParts[3]) - decimal.Parse(dataParts[2]),
                ChangePercent = (decimal.Parse(dataParts[3]) - decimal.Parse(dataParts[2])) / decimal.Parse(dataParts[2]) * 100,
                LastUpdated = DateTime.Now
            };

            return stockData;
        }
    }
}