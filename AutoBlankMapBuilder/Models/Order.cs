using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoBlankMapBuilder.Models
{
    public class Order
    {
        public string Department { get; set; }  //  部署コード
        public string No { get; set; }          //  オーダーNO
        public string Item { get; set; }        //  KISYU
        public string Date { get; set; }        //  投入日
        public int Quantity { get; set; }       //  投入数量主
    }
}
