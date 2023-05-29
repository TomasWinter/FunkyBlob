using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNKY
{
    public class Rap
    {
        // Natavení classy Rap, která má v sobě údaje ze souboru
        public string RapName { get; set; }
        public List<List<int>> ArrowTimeData { get; set; }
        public Uri SongUri { get; set; }
        public Rap(Uri songUri,string name,List<List<int>> arrowTimeData)
        {
            ArrowTimeData = arrowTimeData;
            SongUri = songUri;
            RapName = name;
        }
        public override string ToString()
        {
            return RapName;
        }
    }
}
