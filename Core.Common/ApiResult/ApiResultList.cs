﻿using System;

namespace Core.Common.ApiResult
{
    public class ApiResultList
    {
        public string Message { get; set; }
        public object Result { get; set; }
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public DateTime ResultDate => DateTime.Now;
    }

    public class ApiResultList<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public DateTime ResultDate => DateTime.Now;
    }
}