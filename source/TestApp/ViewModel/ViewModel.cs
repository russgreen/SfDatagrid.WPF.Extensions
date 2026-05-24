using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TestApp.Model;

namespace TestApp.ViewModel;

internal partial class ViewModel : ObservableValidator
{
    [ObservableProperty]
    private ObservableCollection<OrderInfo> _orderInfoCollection;

    public ViewModel()
    {
        OrderInfoCollection = new ObservableCollection<OrderInfo>( GenerateOrders1());
    }

    private List<OrderInfo> GenerateOrders1()
    {
        List<OrderInfo> orders = new List<OrderInfo>();
        for (int i = 0; i < 2; i++)
        {
            orders.Add(new OrderInfo("Bulk 01", "Beverton", "ALFKI", "US", 10));
            orders.Add(new OrderInfo("Oliver", "Oregon", "ANATR", "us", 5));
            orders.Add(new OrderInfo("Brendon", "Johanesberg", "ANTON", "China", 8));
            orders.Add(new OrderInfo("John", "Chicago", "YHGTR", "UK", 12));
            orders.Add(new OrderInfo("Charles", "Spain", "BERGS", "China", 7));
            orders.Add(new OrderInfo("Dintin", "Britain", "TGVFD", "US", 9));
            orders.Add(new OrderInfo("Friedo", "Britain", "YTREW", "US", 6));
            orders.Add(new OrderInfo("John", "Oregon", "MNBGY", "Mumbai", 11));
            orders.Add(new OrderInfo("Sirert", "Bulk", "BGFDE", "US", 4));
            orders.Add(new OrderInfo("Rakesh", "hertion", "BOTTM", "Britain", 3));
        }



        return orders;
    }
}
