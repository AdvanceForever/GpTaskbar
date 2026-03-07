using System;

namespace GpTaskbar.Models
{
    public class StockData
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public DateTime LastUpdated { get; set; }

        public string GetDisplayText(bool showSymbol = true, bool showName = false, bool showPrice = true, bool showChangePercent = true, bool showChange = true)
        {
            var parts = new List<string>();
            
            if (showSymbol)
                parts.Add(Symbol);
                
            if (showName)
                parts.Add(Name);
                
            if (showPrice)
                parts.Add($"{CurrentPrice:F2}");
                
            if (showChange)
                parts.Add($"{ChangeSign}{Math.Abs(Change):F2}");
                
            if (showChangePercent)
                parts.Add($"({ChangeSign}{Math.Abs(ChangePercent):F2}%)");
            
            return string.Join(" ", parts);
        }
        
        public string DisplayText => GetDisplayText();
        
        private string ChangeSign => Change >= 0 ? "+" : "-";
        
        public bool IsRising => Change >= 0;
    }
}