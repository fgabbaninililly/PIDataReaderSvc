using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDataReaderSvc {
	public class Version {
		public static readonly string version = "1.2.1";

		public static string getVersion() {
			return string.Format("PIDataReader Service Launcher v{0}", version);
		}
	}
}
