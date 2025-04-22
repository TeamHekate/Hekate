using System;

namespace Frontend.Models;

public class MemoryGridPageModel
{
    public MemoryGridRowModel[] Rows { get; }

    public MemoryGridPageModel(byte page, Span<byte> rows)
    {
        Rows = new MemoryGridRowModel[16];
        for (byte r = 0; r < 16; r++) ;
        // Rows[r] = new MemoryGridRowModel(page, r, rows.Slice(r * 16, 16));
    }
}