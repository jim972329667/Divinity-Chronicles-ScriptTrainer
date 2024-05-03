using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptTrainer.Cards
{
    public class ModCardValue
    {
        public int IntValue = 0;
        public float FloatValue = 0;
        public string StringValue = string.Empty;

        public object _value = null;
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                StringValue = value.ToString();
                int.TryParse(StringValue, out IntValue );
                float.TryParse(StringValue, out FloatValue );
            }
        }
       
        public ModCardValue(object value)
        {
            Value = value;
        }
    }
}
