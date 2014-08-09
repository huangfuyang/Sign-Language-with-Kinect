using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;

namespace SignLanguageEducationSystem {

	public class SystemStatusCollection : AutoNotifyPropertyChanged {

		private bool _isKinectAllSet;
		public bool IsKinectAllSet {
			get { return _isKinectAllSet; }
			set { SetProperty(ref _isKinectAllSet, value, true); }
		}

		private SignWord _currentSignWord;
		public SignWord CurrentSignWord {
			get { return _currentSignWord; }
			set { SetProperty(ref _currentSignWord, value, true); }
		}

		private KinectSensor _currentKinectSensor;
		public KinectSensor CurrentKinectSensor {
			get { return _currentKinectSensor; }
			set { SetProperty(ref _currentKinectSensor, value, true); }
		}

		private WriteableBitmap _colorBitmap;
		public WriteableBitmap ColorBitmap {
			get { return _colorBitmap; }
			set { SetProperty(ref _colorBitmap, value, true); }
		}

		/* Input Format 
		 *	Column 1 (#): Unique ID from 1
		 *	Column 2 (Sign ID)
]		 *	Column 3 (Chinese Name)
		 *	Column 4 (English Name)
		 *	Column 5 (Learn Time): Number of time learning the sign
		 */
		public DataTable SignWordTable { get; private set; }

		public SystemStatusCollection() {
			loadSignWordTable("Data/Input.txt");
		}

		public void loadSignWordTable(string path) {
			if (SignWordTable != null) {
				SignWordTable.Clear();
			} else {
				SignWordTable = new DataTable();
				SignWordTable.Columns.Add("#", typeof(int));
				SignWordTable.Columns.Add("Sign ID");
				SignWordTable.Columns.Add("Chinese Name");
				SignWordTable.Columns.Add("English Name");
				SignWordTable.Columns.Add("Learn Time", typeof(int));
			}

			using (StreamReader reader = new StreamReader(File.OpenRead(path), Encoding.UTF8)) {
				string line;

				while ((line = reader.ReadLine()) != null) {
					DataRow row = SignWordTable.NewRow();
					int intValue;

					string[] attributes = line.Split('\t');
					for (int i = 0; i < attributes.Length; i++) {
						if (attributes[i] == string.Empty)
							continue;
						if (int.TryParse(attributes[i], out intValue)) {
							row[i] = intValue;
						} else {
							row[i] = attributes[i];
						}
					}

					row["Learn Time"] = 0;
					SignWordTable.Rows.Add(row);
				}
			}
		}
	}
}
