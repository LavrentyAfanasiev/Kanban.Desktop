﻿using System;

namespace Data.Entities.Common.LocalBase
{
    public class BoardInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Config { get; set; }
    }
}
