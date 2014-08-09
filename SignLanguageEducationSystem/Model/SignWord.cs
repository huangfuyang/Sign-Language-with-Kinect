using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguageEducationSystem {
	public class SignWord {
		public String Name {
			get;
			private set;
		}

		public string ID {
			get;
			private set;
		}
		public string Path {
			get {
				return "Data/Videos/" + ID + ".avi"; }
			private set { }
		}

		public SignWord(String name, String id) {
			this.Name = name;
			this.ID = id;
		}
	}
}
