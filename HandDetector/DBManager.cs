using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Common;
using System.Windows.Forms;

namespace CURELab.SignLanguage.HandDetector
{
    public class DBManager
    {
        private SQLiteConnection connection { get; set; }
        private SQLiteCommand command { get; set; }
        private List<string> insertions { get; set; }

        public String SequenceName { get; set; }
        public String DataSource { get; set; }
        public String RecordingName { get; set; }
        public String Signer { get; set; }
        public String RecordIndex { get; set; }
        public bool UseSeqRecordIndex { get; set; } //Use sequence file record index, or use from user input
        public bool UseTimestamp { get; set; } //Use timestamp to count frame, instead of frame number

        private bool _begin = false;
        private Timer timer;
        public bool Begin
        {
            get { return _begin; }
            set
            {
                if (value)
                {
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                }
                _begin = value;
            }
        }
        public long CurrentSign = 0;
        public int currentFrame = 0;
        private static DBManager singleton;

        private SQLiteTransaction tran;
        public static DBManager GetSingleton(string dataSource)
        {
            if (singleton == null || singleton.DataSource != dataSource)
            {
                singleton = new DBManager(dataSource);
                return singleton;

            }
            return singleton;
        }

        public static DBManager GetSingleton()
        {
            return singleton;
        }

        private DBManager(string dataSource)
        {
            this.DataSource = dataSource;
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
            connection = new SQLiteConnection(String.Format("Data source={0};Version=3;PRAGMA synchronous=off", DataSource));
            connection.Open();
            if (connection != null)
            {
                //CreateDatabase();
            }

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            command = connection.CreateCommand();

        }

        public void CreateDatabase()
        {
            SQLiteTransaction trans = connection.BeginTransaction();
            try
            {
                string sqlSignTable =
                   @"CREATE TABLE IF NOT EXISTS [SignWord] (
                          [SignID] TEXT NOT NULL PRIMARY KEY,
                          [SignIndex] INTEGER NOT NULL,
                          [Chinese] TEXT  NULL,
                          [English] TEXT  NULL,
                          [Count] INTEGER NOT NULL
                          )";

                string sqlSampleTable =
                   @"CREATE TABLE IF NOT EXISTS [SignSample] (
                          [index_ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                          [SignID] TEXT NOT NULL,
                          [Signer] TEXT NOT NULL,
                          [FileName] TEXT NOT  NULL,
                          [Intersected] INTEGER NOT NULL,
                          FOREIGN KEY(SignID) REFERENCES SignWord(SignID)
                          )";
                string sqlDataTable =
                   @"CREATE TABLE IF NOT EXISTS [FrameData] (
                          [index_ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                          [SampleIndex] INTEGER NOT NULL,
                          [FrameNumber] INTEGER NOT NULL,
                           SkeletonHeadX NUMERIC,
	                        SkeletonHeadY NUMERIC,
	                        SkeletonHeadZ NUMERIC,
	                        SkeletonShoulderCenterX NUMERIC,
	                        SkeletonShoulderCenterY NUMERIC,
	                        SkeletonShoulderCenterZ NUMERIC,
	                        SkeletonShoulderLeftX NUMERIC,
	                        SkeletonShoulderLeftY NUMERIC,
	                        SkeletonShoulderLeftZ NUMERIC,
	                        SkeletonShoulderRightX NUMERIC,
	                        SkeletonShoulderRightY NUMERIC,
	                        SkeletonShoulderRightZ NUMERIC,
	                        SkeletonSpineX NUMERIC,
	                        SkeletonSpineY NUMERIC,
	                        SkeletonSpineZ NUMERIC,
	                        SkeletonHipCenterX NUMERIC,
	                        SkeletonHipCenterY NUMERIC,
	                        SkeletonHipCenterZ NUMERIC,
	                        SkeletonHipLeftX NUMERIC,
	                        SkeletonHipLeftY NUMERIC,
	                        SkeletonHipLeftZ NUMERIC,
	                        SkeletonHipRightX NUMERIC,
	                        SkeletonHipRightY NUMERIC,
	                        SkeletonHipRightZ NUMERIC,
	                        SkeletonElbowLeftX NUMERIC,
	                        SkeletonElbowLeftY NUMERIC,
	                        SkeletonElbowLeftZ NUMERIC,
	                        SkeletonWristLeftX NUMERIC,
	                        SkeletonWristLeftY NUMERIC,
	                        SkeletonWristLeftZ NUMERIC,
	                        SkeletonHandLeftX NUMERIC,
	                        SkeletonHandLeftY NUMERIC,
	                        SkeletonHandLeftZ NUMERIC,
	                        SkeletonElbowRightX NUMERIC,
	                        SkeletonElbowRightY NUMERIC,
	                        SkeletonElbowRightZ NUMERIC,
	                        SkeletonWristRightX NUMERIC,
	                        SkeletonWristRightY NUMERIC,
	                        SkeletonWristRightZ NUMERIC,
	                        SkeletonHandRightX NUMERIC,
	                        SkeletonHandRightY NUMERIC,
	                        SkeletonHandRightZ NUMERIC,
                          [RightHandHOG] BLOB ,
                          [LeftHandHOG] BLOB ,
                          RightMoG BLOB,
                          LeftMoG BLOB,
                          FOREIGN KEY(SampleIndex) REFERENCES SignSample(index_ID)
                          )";

                SQLiteCommand createTable = new SQLiteCommand(sqlSignTable, connection);
                createTable.ExecuteNonQuery();
                createTable = new SQLiteCommand(sqlSampleTable, connection);
                createTable.ExecuteNonQuery();
                createTable = new SQLiteCommand(sqlDataTable, connection);
                createTable.ExecuteNonQuery();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }


        int preFrame = int.MinValue;
        void timer_Tick(object sender, EventArgs e)
        {
            if (currentFrame - preFrame == 0)
            {
                Begin = false;
                this.Commit();
            }
            preFrame = currentFrame;
        }

        public SignWordModel CurrentModel;
        public void AddWordSample(SignWordModel wordModel)
        {

            try
            {
                SQLiteCommand insertCommand = new SQLiteCommand(connection);

                string insertSample =
                   @"INSERT INTO SignSample (SignID, Signer,FileName,Intersected) VALUES
                                    (@SignID, @Signer ,@File,0)";
                insertCommand = new SQLiteCommand(insertSample, connection);
                insertCommand.Parameters.AddWithValue("@SignID", wordModel.SignID);
                insertCommand.Parameters.AddWithValue("@Signer", wordModel.Signer);
                insertCommand.Parameters.AddWithValue("@File", wordModel.File);
                insertCommand.ExecuteNonQuery();
                string sql = @"select last_insert_rowid()";
                insertCommand.CommandText = sql;
                long lastId = (long)insertCommand.ExecuteScalar();
                CurrentSign = lastId;
                currentFrame = 0;
                CurrentModel = wordModel;
                // update count
                string update = @"UPDATE SignWord 
                set count = count +1
                where signID = @ID ";
                SQLiteCommand updateCommand = new SQLiteCommand(update, connection);
                updateCommand.Parameters.AddWithValue("@ID", wordModel.SignID);
                //Console.WriteLine(updateCommand.CommandText);
                updateCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                tran.Rollback();
                throw;
            }

        }

        public void UpdateMogData(int frame, float[] mog, bool isRight)
        {
            try
            {
                 SQLiteCommand updateCommand = new SQLiteCommand(connection);
                 string updateFrame;
                 if (isRight)
                 {
                     updateFrame =
                      @"UPDATE Framedata SET mogRight = @mog
                         where index_id = @frame";
                 }
                 else
                 {
                     updateFrame =
                     @"UPDATE Framedata SET mogLeft = @mog
                        where index_id = @frame";
                 }
                
                updateCommand.CommandText = updateFrame;
                updateCommand.Parameters.AddWithValue("@frame", frame);
                updateCommand.Parameters.Add("@mog", DbType.Binary, mog.Length*sizeof(float)).Value = mog.ToByteArray();
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                
                throw;
            }
        }

        public void AddWordModel(SignWordModel wordModel, int index)
        {


            try
            {
                SQLiteCommand insertCommand = new SQLiteCommand(connection);

                string insertWord =
                 @"INSERT  OR REPLACE INTO SignWord (SignID,SignIndex,Chinese,English,Count) VALUES" +
                 "(@ID,@Index, @Chinese, @English,0)";
                insertCommand = new SQLiteCommand(insertWord, connection);
                insertCommand.Parameters.AddWithValue("@ID", wordModel.SignID);
                insertCommand.Parameters.AddWithValue("@Index", index);
                insertCommand.Parameters.AddWithValue("@Chinese", wordModel.Chinese);
                insertCommand.Parameters.AddWithValue("@English", wordModel.English);
                insertCommand.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                tran.Rollback();
                throw;
            }
        }


        public bool AddFrameData(HandShapeModel hand)
        {
            currentFrame++;
            if (!Begin || hand == null)
            {
                return false;
            }
            try
            {
                SQLiteCommand insertCommand = new SQLiteCommand(connection);
                string insertframe =
                String.Format("INSERT  OR REPLACE INTO FrameData (" +
                   "SampleIndex,FrameNumber," +
                   "SkeletonHeadX, SkeletonHeadY, SkeletonHeadZ, " +
                   "SkeletonShoulderCenterX, SkeletonShoulderCenterY, SkeletonShoulderCenterZ, " +
                   "SkeletonShoulderLeftX, SkeletonShoulderLeftY, SkeletonShoulderLeftZ, " +
                   "SkeletonShoulderRightX, SkeletonShoulderRightY, SkeletonShoulderRightZ, " +
                   "SkeletonSpineX, SkeletonSpineY, SkeletonSpineZ, " +
                   "SkeletonHipCenterX, SkeletonHipCenterY, SkeletonHipCenterZ, " +
                   "SkeletonHipLeftX, SkeletonHipLeftY, SkeletonHipLeftZ, " +
                   "SkeletonHipRightX, SkeletonHipRightY, SkeletonHipRightZ, " +
                   "SkeletonElbowLeftX, SkeletonElbowLeftY, SkeletonElbowLeftZ, " +
                   "SkeletonWristLeftX, SkeletonWristLeftY, SkeletonWristLeftZ, " +
                   "SkeletonHandLeftX, SkeletonHandLeftY, SkeletonHandLeftZ, " +
                   "SkeletonElbowRightX, SkeletonElbowRightY, SkeletonElbowRightZ, " +
                   "SkeletonWristRightX, SkeletonWristRightY, SkeletonWristRightZ, " +
                   "SkeletonHandRightX, SkeletonHandRightY, SkeletonHandRightZ, " +
                   "RightHandHOG, LeftHandHOG) VALUES" +
                   "(@SampleID, @Frame" +
                   "{0},@Right,@Left)", hand.skeletonData);




                insertCommand = new SQLiteCommand(insertframe, connection);
                insertCommand.Parameters.AddWithValue("@SampleID", CurrentSign);
                insertCommand.Parameters.AddWithValue("@Frame", currentFrame);
                if (hand.hogRight == null || hand.hogRight.Length == 0)
                {
                    insertCommand.Parameters.AddWithValue("@Right", null);
                }
                else
                {
                    insertCommand.Parameters.Add("@Right", DbType.Binary, hand.hogRight.Length * sizeof(float)).Value = hand.hogRight.ToByteArray();
                }
                if (hand.hogLeft == null || hand.hogLeft.Length == 0)
                {
                    insertCommand.Parameters.AddWithValue("@Left", null);
                }
                else
                {
                    insertCommand.Parameters.Add("@Left", DbType.Binary, hand.hogLeft.Length * sizeof(float)).Value = hand.hogLeft.ToByteArray();
                }
                insertCommand.ExecuteNonQuery();
                string sql = @"select last_insert_rowid()";
                insertCommand.CommandText = sql;
                long lastId = (long)insertCommand.ExecuteScalar();
                hand.frame = lastId;
                // update sign type
                if (hand.type == HandEnum.Intersect)
                {
                    string update = @"UPDATE signsample 
                set intersected = 1 
                where index_id = @ID ";
                    SQLiteCommand updateCommand = new SQLiteCommand(update, connection);
                    updateCommand.Parameters.AddWithValue("@ID", CurrentSign);
                    //Console.WriteLine(updateCommand.CommandText);
                    updateCommand.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception e)
            {
                tran.Rollback();
                return false;
            }

        }

        public void BeginTrans()
        {
            if (connection != null)
            {
                tran = connection.BeginTransaction();
            }
        }


        public void Commit()
        {
            if (tran != null)
            {
                tran.Commit();
            }
        }

        public void Test()
        {
            command = new SQLiteCommand(connection);
            command.CommandText = "SELECT RighthandHOG FROM FrameData WHERE index_id = 1";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    byte[] buffer = GetBytes(reader);
                    float a = BitConverter.ToSingle(buffer, 0);
                }
            }
        }
        public byte[] GetBytes(SQLiteDataReader reader)
        {
            const int CHUNK_SIZE = 4500;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        public void WriteFrameData(HandShapeModel handModel)
        {
            if (connection == null || command == null) return;




            //    cmd = String.Format("INSERT INTO FrameData " +
            //        "(FrameDataId, SignInfoId, FrameCount, OffsetedFrameCount, " + //0, 1, 2
            //"SkeletonHeadX, SkeletonHeadY, SkeletonHeadZ, " +
            //"SkeletonShoulderCenterX, SkeletonShoulderCenterY, SkeletonShoulderCenterZ, " +
            //"SkeletonShoulderLeftX, SkeletonShoulderLeftY, SkeletonShoulderLeftZ, " +
            //"SkeletonShoulderRightX, SkeletonShoulderRightY, SkeletonShoulderRightZ, " +
            //"SkeletonSpineX, SkeletonSpineY, SkeletonSpineZ, " +
            //"SkeletonHipCenterX, SkeletonHipCenterY, SkeletonHipCenterZ, " +
            //"SkeletonHipLeftX, SkeletonHipLeftY, SkeletonHipLeftZ, " +
            //"SkeletonHipRightX, SkeletonHipRightY, SkeletonHipRightZ, " +
            //"SkeletonElbowLeftX, SkeletonElbowLeftY, SkeletonElbowLeftZ, " +
            //"SkeletonWristLeftX, SkeletonWristLeftY, SkeletonWristLeftZ, " +
            //"SkeletonHandLeftX, SkeletonHandLeftY, SkeletonHandLeftZ, " +
            //"SkeletonElbowRightX, SkeletonElbowRightY, SkeletonElbowRightZ, " +
            //"SkeletonWristRightX, SkeletonWristRightY, SkeletonWristRightZ, " +
            //"SkeletonHandRightX, SkeletonHandRightY, SkeletonHandRightZ, " +
            //        "HandCount, " +
            //        "Hand0FingertipCount, " +
            //        "Hand0Fingertip0X, Hand0Fingertip0Y, Hand0Fingertip0Z, " +
            //        "Hand0Fingertip1X, Hand0Fingertip1Y, Hand0Fingertip1Z, " +
            //        "Hand0Fingertip2X, Hand0Fingertip2Y, Hand0Fingertip2Z, " +
            //        "Hand0Fingertip3X, Hand0Fingertip3Y, Hand0Fingertip3Z, " +
            //        "Hand0Fingertip4X, Hand0Fingertip4Y, Hand0Fingertip4Z, " +
            //        "Hand0EllipseCenterX, Hand0EllipseCenterY, " +
            //        "Hand0EllipseMajorAxis, Hand0EllipseMinorAxis, " +
            //        "Hand0EllipseAspectRatio, Hand0AxisTheta, " +
            //        "Hand1FingertipCount, " +
            //        "Hand1Fingertip0X, Hand1Fingertip0Y, Hand1Fingertip0Z, " +
            //        "Hand1Fingertip1X, Hand1Fingertip1Y, Hand1Fingertip1Z, " +
            //        "Hand1Fingertip2X, Hand1Fingertip2Y, Hand1Fingertip2Z, " +
            //        "Hand1Fingertip3X, Hand1Fingertip3Y, Hand1Fingertip3Z, " +
            //        "Hand1Fingertip4X, Hand1Fingertip4Y, Hand1Fingertip4Z, " +
            //        "Hand1EllipseCenterX, Hand1EllipseCenterY, " +
            //        "Hand1EllipseMajorAxis, Hand1EllipseMinorAxis, " +
            //        "Hand1EllipseAspectRatio, Hand1AxisTheta) " +
            //        "VALUES(NULL, {0}, {1}, {2}{3})",
            //        SignInfoId, frameData.frameCount, frameData.OffsetedFrameCount, frameData.GetFrameDataArgString()
            //        );
            //    insertions.Add(cmd);
            //} else {

            //    if (insertions.Count > 0) { //Batch insertion
            //        var sw = Stopwatch.StartNew();
            //        reader.Close();
            //        SQLiteTransaction trans = connection.BeginTransaction(); // <-------------------
            //        try
            //        {
            //            foreach (string insertion in insertions)
            //            {
            //                command.CommandText = insertion;
            //                command.ExecuteNonQuery();
            //            }
            //            insertions.Clear();
            //            trans.Commit(); // <-------------------
            //        }
            //        catch
            //        {
            //            trans.Rollback(); // <-------------------
            //            throw; // <-------------------
            //        }

            //        Console.WriteLine("save data:" + sw.ElapsedMilliseconds);
            //    }
            //}
            //if (!reader.IsClosed) {
            //    reader.Close();
            //}
            ////if (cmd != String.Empty) {
            ////    command.CommandText = cmd;
            ////    command.ExecuteNonQuery();
            ////}
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }
    }
}
