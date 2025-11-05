using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.DTOs.Products.Responses;
using System;
using System.Collections.Generic;

namespace SakuraHomeAPI.DTOs.Admin.Responses
{
    /// <summary>
    /// Complete dashboard overview data
    /// </summary>
    public class DashboardOverviewDto
    {
        public AdminUserStatisticsDto? UserStats { get; set; }
        public OrderStatsDto? OrderStats { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; } = new();
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int PendingOrders { get; set; }
    }

    /// <summary>
    /// Revenue analytics data
    /// </summary>
    public class RevenueAnalyticsDto
    {
        public string Period { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Daily revenue breakdown
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Product statistics data
    /// </summary>
    //public class ProductStatsDto
    //{
    //    public int TotalProducts { get; set; }
    //    public int ActiveProducts { get; set; }
    //    public int InactiveProducts { get; set; }
    //    public int OutOfStockProducts { get; set; }
    //    public int LowStockProducts { get; set; }
    //}
}