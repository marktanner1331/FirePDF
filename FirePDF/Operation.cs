﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Operation
    {
        public string operatorName;
        public List<object> operands;

        public Operation()
        {
            operands = new List<object>();
        }
    }
}
