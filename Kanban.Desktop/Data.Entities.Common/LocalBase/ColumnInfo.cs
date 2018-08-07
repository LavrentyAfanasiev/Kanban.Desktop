﻿namespace Data.Entities.Common.LocalBase
{
    public class ColumnInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte Order { get; set; }
        public BoardInfo Board { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
