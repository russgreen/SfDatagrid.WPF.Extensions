using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp.Model;

internal partial class OrderInfo : ObservableObject
{
    [ObservableProperty]
    private string _customerId;

    [ObservableProperty]
    private string _country;

    [ObservableProperty]
    private string _customerName;

    [ObservableProperty]
    private string _shippingCity;

    public OrderInfo(string customerName, string country, string
        customerId, string shipCity)
    {
        CustomerName = customerName;
        Country = country;
        CustomerId = customerId;
        ShippingCity = shipCity;
    }
}
