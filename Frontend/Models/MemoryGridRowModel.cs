using System;

namespace Frontend.Models;

public class MemoryGridRowModel(string highNibble, string[] locations)
{
    public string HighNibble { get; set; } = highNibble;
    public string[] Locations { get; set; } = locations;
}