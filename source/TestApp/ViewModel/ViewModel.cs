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
            orders.Add(new OrderInfo("Bulk 1", "Beverton", "ALFKI", "US"));
            orders.Add(new OrderInfo("Oliver", "Oregon", "ANATR", "us"));
            orders.Add(new OrderInfo("Brendon", "Johanesberg", "ANTON", "China"));
            orders.Add(new OrderInfo("John", "Chicago", "YHGTR", "UK"));
            orders.Add(new OrderInfo("Charles", "Spain", "BERGS", "China"));
            orders.Add(new OrderInfo("Dintin", "Britain", "TGVFD", "US"));
            orders.Add(new OrderInfo("Friedo", "Britain", "YTREW", "US"));
            orders.Add(new OrderInfo("John", "Oregon", "MNBGY", "Mumbai"));
            orders.Add(new OrderInfo("Sirert", "Bulk", "BGFDE", "US"));
            orders.Add(new OrderInfo("Rakesh", "hertion", "BOTTM", "Britain"));
        }



        return orders;
    }
}
