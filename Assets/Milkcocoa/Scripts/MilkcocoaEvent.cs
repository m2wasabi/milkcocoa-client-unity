namespace Milkcocoa
{
    public class MilkcocoaEvent
    {
        public string name { get; set; }
        public JSONObject data { get; set; }
        public MilkcocoaEvent(string name) : this(name,null) { }

        public MilkcocoaEvent(string name, JSONObject data)
        {
            this.name = name;
            this.data = data;
        }

        public override string ToString()
        {
            return string.Format("[MilkcocoaEvent({0}) data={1}]",name,data);
        }
    }
}