using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PGTest {
    class Program {
        private static LicenseInitializer m_AOLicenseInitializer = new PGTest.LicenseInitializer();
        const string HOST = "192.168.1.100";
        const int PORT = 5432;
        const string USER = "postgres";
        const string PASSWORD = "admin";
        const string DB = "postgis_rcl";

        //const string HOST = "192.168.206.100";
        //const int PORT = 5432;
        //const string USER = "gpadmin";
        //const string PASSWORD = "";
        //const string DB = "gis_db";

        static string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

        static void Main(string[] args) {
            //m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeEngine },
            //new esriLicenseExtensionCode[] { });


            //CreateTable();
            //ImportEDGES(@"E:\SpatialHadoop\EDGES.csv\EDGES.csv");
            ReadCsvFile(@"E:\OpenStreetMap\buildings\buildings");
            //Thread();

            //m_AOLicenseInitializer.ShutdownApplication();
        }

        static void Thread() {
            new System.Threading.Thread(Thread1).Start();
            new System.Threading.Thread(Thread2).Start();
            new System.Threading.Thread(Thread3).Start();
            new System.Threading.Thread(Thread4).Start();
            new System.Threading.Thread(Thread5).Start();
        }

        static void Thread1() {
            ImportThread(0, 15000000, @"E:\SpatialHadoop\EDGES.csv\EDGES.csv", @"C:\data\Log1.txt");
        }
        static void Thread2() {
            ImportThread(15000001, 30000000, @"E:\SpatialHadoop\EDGES.csv\EDGES.csv", @"C:\data\Log2.txt");
        }
        static void Thread3() {
            ImportThread(30000001, 45000000, @"E:\SpatialHadoop\EDGES.csv\EDGES.csv", @"C:\data\Log3.txt");
        }
        static void Thread4() {
            ImportThread(45000001, 60000000, @"E:\SpatialHadoop\EDGES.csv\EDGES.csv", @"C:\data\Log4.txt");
        }
        static void Thread5() {
            ImportThread(60000001, 80000000, @"E:\SpatialHadoop\EDGES.csv\EDGES.csv", @"C:\data\Log5.txt");
        }

        static void ReadCsvFile(string csvPath) {
            using (System.IO.FileStream fs = new System.IO.FileStream(csvPath, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        Log(@"C:\Users\QWE\Desktop\temp\building.txt", line + "\r\n\r\n\r\n");
                    }
                }
            }
        }



        static void ImportThread(long start, long end, string csvPath, string logPath) {
            using (System.IO.FileStream fs = new System.IO.FileStream(csvPath, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8)) {
                    using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                        conn.Open();
                        using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                            cmd.Connection = conn;
                            NpgsqlTransaction trans = conn.BeginTransaction();
                            string log = "Start[" + start + "," + end + "]:" + DateTime.Now.ToString("HH:mm:ss fff");
                            Console.WriteLine(log);
                            Log(logPath, log);
                            long lineNum = 0;
                            string line;
                            FLine_EDGES model = new FLine_EDGES();
                            while ((line = sr.ReadLine()) != null) {
                                lineNum++;
                                if (lineNum < start) {
                                    continue;
                                }
                                if (lineNum > end) {
                                    break;
                                }
                                string[] splits = SplitAndCheckLine_EDGES(line, lineNum);
                                if (splits == null) {
                                    continue;
                                }
                                model = Line2Model(model, splits);

                                string sql = "INSERT INTO \"edges\"(\"id\",\"statefp\",\"countyfp\",\"tlid\",\"tfidl\",\"tfidr\",\"mtfcc\",\"fullname\",\"smid\",\"lfromadd\",\"ltoadd\",\"rfromadd\",\"rtoadd\",\"zipl\",\"zipr\",\"featcat\",\"hydroflg\",\"railflg\",\"roadflg\",\"olfflg\",\"passflg\",\"divroad\",\"exttyp\",\"ttyp\",\"deckedroad\",\"artpath\",\"persist\",\"gcseflg\",\"offsetl\",\"offsetr\",\"tnidf\",\"tnidt\",\"geom\") VALUES (@para1,@para2,@para3,@para4,@para5,@para6,@para7,@para8,@para9,@para10,@para11,@para12,@para13,@para14,@para15,@para16,@para17,@para18,@para19,@para20,@para21,@para22,@para23,@para24,@para25,@para26,@para27,@para28,@para29,@para30,@para31,@para32,ST_GeomFromText(@para33,4326))";

                                NpgsqlParameter[] npc = {
                                    new NpgsqlParameter("@para1",DbType.Int64) { Value=lineNum},
                                    new NpgsqlParameter("@para2",DbType.String) { Value=model.STATEFP},
                                    new NpgsqlParameter("@para3",DbType.String) { Value=model.COUNTYFP},
                                    new NpgsqlParameter("@para4",DbType.String) { Value=model.TLID},
                                    new NpgsqlParameter("@para5",DbType.String) { Value=model.TFIDL},
                                    new NpgsqlParameter("@para6",DbType.String) { Value=model.TFIDR},
                                    new NpgsqlParameter("@para7",DbType.String) { Value=model.MTFCC},
                                    new NpgsqlParameter("@para8",DbType.String) { Value=model.FULLNAME},
                                    new NpgsqlParameter("@para9",DbType.String) { Value=model.SMID},
                                    new NpgsqlParameter("@para10",DbType.String) { Value=model.LFROMADD},
                                    new NpgsqlParameter("@para11",DbType.String) { Value=model.LTOADD},
                                    new NpgsqlParameter("@para12",DbType.String) { Value=model.RFROMADD},
                                    new NpgsqlParameter("@para13",DbType.String) { Value=model.RTOADD},
                                    new NpgsqlParameter("@para14",DbType.String) { Value=model.ZIPL},
                                    new NpgsqlParameter("@para15",DbType.String) { Value=model.ZIPR},
                                    new NpgsqlParameter("@para16",DbType.String) { Value=model.FEATCAT},
                                    new NpgsqlParameter("@para17",DbType.String) { Value=model.HYDROFLG},
                                    new NpgsqlParameter("@para18",DbType.String) { Value=model.RAILFLG},
                                    new NpgsqlParameter("@para19",DbType.String) { Value=model.ROADFLG},
                                    new NpgsqlParameter("@para20",DbType.String) { Value=model.OLFFLG},
                                    new NpgsqlParameter("@para21",DbType.String) { Value=model.PASSFLG},
                                    new NpgsqlParameter("@para22",DbType.String) { Value=model.DIVROAD},
                                    new NpgsqlParameter("@para23",DbType.String) { Value=model.EXTTYP},
                                    new NpgsqlParameter("@para24",DbType.String) { Value=model.TTYP},
                                    new NpgsqlParameter("@para25",DbType.String) { Value=model.DECKEDROAD},
                                    new NpgsqlParameter("@para26",DbType.String) { Value=model.ARTPATH},
                                    new NpgsqlParameter("@para27",DbType.String) { Value=model.PERSIST},
                                    new NpgsqlParameter("@para28",DbType.String) { Value=model.GCSEFLG},
                                    new NpgsqlParameter("@para29",DbType.String) { Value=model.OFFSETL},
                                    new NpgsqlParameter("@para30",DbType.String) { Value=model.OFFSETR},
                                    new NpgsqlParameter("@para31",DbType.String) { Value=model.TNIDF},
                                    new NpgsqlParameter("@para32",DbType.String) { Value=model.TNIDT},
                                    new NpgsqlParameter("@para33",DbType.Object) { Value=model.WKT.Trim('"')},
                                };
                                try {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = sql;
                                    foreach (var item in npc) {
                                        cmd.Parameters.Add(item);
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex) {
                                    log = "[" + lineNum + "] error." + ex.Message;
                                    Console.WriteLine(log);
                                    Log(logPath, log);
                                }
                                if (lineNum % 10000 == 0) {
                                    trans.Commit();
                                    log = "[" + lineNum + "]" + " Transaction." + DateTime.Now.ToString("HH:mm:ss fff");
                                    Console.WriteLine(log);
                                    Log(logPath, log);
                                    trans = conn.BeginTransaction();
                                }
                            }
                            trans.Commit();
                            log = "Sum:" + lineNum;
                            Console.WriteLine(log);
                            Log(logPath, log);
                            trans.Dispose();
                        }
                    }
                }
            }
            string l = "Finish[" + start + "," + end + "]:" + DateTime.Now.ToString("HH:mm:ss fff");
            Console.WriteLine(l);
            Log(logPath, l);
        }

        static void CreateTable() {
            #region 
            string createTableSql = @"CREATE TABLE EDGES(   ID int8,
                                                           STATEFP varchar(2048),
                                                           COUNTYFP varchar(2048),
                                                           TLID varchar(2048),
                                                           TFIDL varchar(2048),
                                                           TFIDR varchar(2048),
                                                           MTFCC varchar(2048),
                                                           FULLNAME varchar(2048),
                                                           SMID varchar(2048),
                                                           LFROMADD varchar(2048),
                                                           LTOADD varchar(2048),
                                                           RFROMADD varchar(2048),
                                                           RTOADD varchar(2048),
                                                           ZIPL varchar(2048),
                                                           ZIPR varchar(2048),
                                                           FEATCAT varchar(2048),
                                                           HYDROFLG varchar(2048),
                                                           RAILFLG varchar(2048),
                                                           ROADFLG varchar(2048),
                                                           OLFFLG varchar(2048),
                                                           PASSFLG varchar(2048),
                                                           DIVROAD varchar(2048),
                                                           EXTTYP varchar(2048),
                                                           TTYP varchar(2048),
                                                           DECKEDROAD varchar(2048),
                                                           ARTPATH varchar(2048),
                                                           PERSIST varchar(2048),
                                                           GCSEFLG varchar(2048),
                                                           OFFSETL varchar(2048),
                                                           OFFSETR varchar(2048),
                                                           TNIDF varchar(2048),
                                                           TNIDT varchar(2048))";
            #endregion
            string addGeoColumnSql = "SELECT AddGeometryColumn ('edges', 'geom', 4326, 'LINESTRING', 2)";
            string dropGeoColumnSql = "SELECT DropGeometryColumn ('edges', 'geom')";

            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = dropGeoColumnSql;
                    try {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("drop geo column success");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("drop geo column" + ex.Message);
                    }
                }
                using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = createTableSql;
                    try {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("create table success");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("create table" + ex.Message);
                    }
                }
                using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = addGeoColumnSql;
                    try {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("add geo column success");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("add geo column" + ex.Message);
                    }
                }
            }
            Console.Read();
        }

        static void ImportEDGES(string csvPath) {
            using (System.IO.FileStream fs = new System.IO.FileStream(csvPath, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8)) {
                    using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                        conn.Open();
                        using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                            cmd.Connection = conn;
                            NpgsqlTransaction trans = conn.BeginTransaction();
                            string log = "Start:" + DateTime.Now.ToString("HH:mm:ss fff");
                            Console.WriteLine(log);
                            Log(log);
                            long lineNum = 0;
                            string line;
                            FLine_EDGES model = new FLine_EDGES();
                            //Stopwatch sw = new Stopwatch();
                            while ((line = sr.ReadLine()) != null) {
                                //sw.Restart();
                                lineNum++;
                                string[] splits = SplitAndCheckLine_EDGES(line, lineNum);
                                if (splits == null) {
                                    continue;
                                }
                                model = Line2Model(model, splits);

                                #region
                                //         string sql = string.Format("INSERT INTO \"edges\"(\"id\",\"statefp\",\"countyfp\",\"tlid\",\"tfidl\",\"tfidr\",\"mtfcc\",\"fullname\",\"smid\",\"lfromadd\",\"ltoadd\",\"rfromadd\",\"rtoadd\",\"zipl\",\"zipr\",\"featcat\",\"hydroflg\",\"railflg\",\"roadflg\",\"olfflg\",\"passflg\",\"divroad\",\"exttyp\",\"ttyp\",\"deckedroad\",\"artpath\",\"persist\",\"gcseflg\",\"offsetl\",\"offsetr\",\"tnidf\",\"tnidt\",\"GEOM\") VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}',ST_GeomFromText('{32}',4326))",
                                //lineNum, model.STATEFP, model.COUNTYFP, model.TLID, model.TFIDL, model.TFIDR, model.MTFCC, model.FULLNAME, model.SMID, model.LFROMADD, model.LTOADD, model.RFROMADD, model.RTOADD, model.ZIPL, model.ZIPR, model.FEATCAT, model.HYDROFLG, model.RAILFLG, model.ROADFLG, model.OLFFLG, model.PASSFLG, model.DIVROAD, model.EXTTYP, model.TTYP, model.DECKEDROAD, model.ARTPATH, model.PERSIST, model.GCSEFLG, model.OFFSETL, model.OFFSETR, model.TNIDF, model.TNIDT, model.WKT.Trim('"'));
                                #endregion

                                string sql = "INSERT INTO \"edges\"(\"id\",\"statefp\",\"countyfp\",\"tlid\",\"tfidl\",\"tfidr\",\"mtfcc\",\"fullname\",\"smid\",\"lfromadd\",\"ltoadd\",\"rfromadd\",\"rtoadd\",\"zipl\",\"zipr\",\"featcat\",\"hydroflg\",\"railflg\",\"roadflg\",\"olfflg\",\"passflg\",\"divroad\",\"exttyp\",\"ttyp\",\"deckedroad\",\"artpath\",\"persist\",\"gcseflg\",\"offsetl\",\"offsetr\",\"tnidf\",\"tnidt\",\"geom\") VALUES (@para1,@para2,@para3,@para4,@para5,@para6,@para7,@para8,@para9,@para10,@para11,@para12,@para13,@para14,@para15,@para16,@para17,@para18,@para19,@para20,@para21,@para22,@para23,@para24,@para25,@para26,@para27,@para28,@para29,@para30,@para31,@para32,ST_GeomFromText(@para33,4326))";

                                NpgsqlParameter[] npc = {
                                    new NpgsqlParameter("@para1",DbType.Int64) { Value=lineNum},
                                    new NpgsqlParameter("@para2",DbType.String) { Value=model.STATEFP},
                                    new NpgsqlParameter("@para3",DbType.String) { Value=model.COUNTYFP},
                                    new NpgsqlParameter("@para4",DbType.String) { Value=model.TLID},
                                    new NpgsqlParameter("@para5",DbType.String) { Value=model.TFIDL},
                                    new NpgsqlParameter("@para6",DbType.String) { Value=model.TFIDR},
                                    new NpgsqlParameter("@para7",DbType.String) { Value=model.MTFCC},
                                    new NpgsqlParameter("@para8",DbType.String) { Value=model.FULLNAME},
                                    new NpgsqlParameter("@para9",DbType.String) { Value=model.SMID},
                                    new NpgsqlParameter("@para10",DbType.String) { Value=model.LFROMADD},
                                    new NpgsqlParameter("@para11",DbType.String) { Value=model.LTOADD},
                                    new NpgsqlParameter("@para12",DbType.String) { Value=model.RFROMADD},
                                    new NpgsqlParameter("@para13",DbType.String) { Value=model.RTOADD},
                                    new NpgsqlParameter("@para14",DbType.String) { Value=model.ZIPL},
                                    new NpgsqlParameter("@para15",DbType.String) { Value=model.ZIPR},
                                    new NpgsqlParameter("@para16",DbType.String) { Value=model.FEATCAT},
                                    new NpgsqlParameter("@para17",DbType.String) { Value=model.HYDROFLG},
                                    new NpgsqlParameter("@para18",DbType.String) { Value=model.RAILFLG},
                                    new NpgsqlParameter("@para19",DbType.String) { Value=model.ROADFLG},
                                    new NpgsqlParameter("@para20",DbType.String) { Value=model.OLFFLG},
                                    new NpgsqlParameter("@para21",DbType.String) { Value=model.PASSFLG},
                                    new NpgsqlParameter("@para22",DbType.String) { Value=model.DIVROAD},
                                    new NpgsqlParameter("@para23",DbType.String) { Value=model.EXTTYP},
                                    new NpgsqlParameter("@para24",DbType.String) { Value=model.TTYP},
                                    new NpgsqlParameter("@para25",DbType.String) { Value=model.DECKEDROAD},
                                    new NpgsqlParameter("@para26",DbType.String) { Value=model.ARTPATH},
                                    new NpgsqlParameter("@para27",DbType.String) { Value=model.PERSIST},
                                    new NpgsqlParameter("@para28",DbType.String) { Value=model.GCSEFLG},
                                    new NpgsqlParameter("@para29",DbType.String) { Value=model.OFFSETL},
                                    new NpgsqlParameter("@para30",DbType.String) { Value=model.OFFSETR},
                                    new NpgsqlParameter("@para31",DbType.String) { Value=model.TNIDF},
                                    new NpgsqlParameter("@para32",DbType.String) { Value=model.TNIDT},
                                    new NpgsqlParameter("@para33",DbType.Object) { Value=model.WKT.Trim('"')},
                                };

                                //sw.Stop();
                                //Console.WriteLine(sw.ElapsedMilliseconds);
                                //sw.Restart();

                                try {
                                    cmd.Parameters.Clear();
                                    //sw.Stop();
                                    //Console.WriteLine("Clear:"+sw.ElapsedMilliseconds);
                                    //sw.Restart();
                                    cmd.CommandText = sql;
                                    foreach (var item in npc) {
                                        cmd.Parameters.Add(item);
                                    }
                                    //sw.Stop();
                                    //Console.WriteLine("text,para:" + sw.ElapsedMilliseconds);
                                    //sw.Restart();
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex) {
                                    log = "[" + lineNum + "] error." + ex.Message;
                                    Console.WriteLine(log);
                                    Log(log);
                                }
                                //sw.Stop();
                                //Console.WriteLine("execute:"+sw.ElapsedMilliseconds);
                                //Console.WriteLine("");
                                if (lineNum % 10000 == 0) {
                                    trans.Commit();
                                    log = "[" + lineNum + "]" + " Transaction." + DateTime.Now.ToString("HH:mm:ss fff");
                                    Console.WriteLine(log);
                                    Log(log);
                                    trans = conn.BeginTransaction();
                                }
                            }
                            trans.Commit();
                            log = "[" + lineNum + "]" + " Transaction." + DateTime.Now.ToString("HH:mm:ss fff");
                            Console.WriteLine(log);
                            Log(log);
                            trans.Dispose();

                            log = "Sum:" + lineNum;
                            Console.WriteLine(log);
                            Log(log);
                        }
                    }
                }
            }
        }

        static string[] SplitAndCheckLine_EDGES(string line, long lineNum) {
            string[] splits = line.Split('\t');
            if (splits.Length != 32) {
                string log = "[" + lineNum.ToString() + "],can not split to 32";
                Console.WriteLine(log);
                Log(log);
                return null;
            }
            return splits;
        }

        static FLine_EDGES Line2Model(FLine_EDGES FModel, string[] splites) {
            FModel.WKT = splites[0]; FModel.STATEFP = splites[1]; FModel.COUNTYFP = splites[2]; FModel.TLID = splites[3]; FModel.TFIDL = splites[4]; FModel.TFIDR = splites[5]; FModel.MTFCC = splites[6]; FModel.FULLNAME = splites[7]; FModel.SMID = splites[8]; FModel.LFROMADD = splites[9]; FModel.LTOADD = splites[10]; FModel.RFROMADD = splites[11]; FModel.RTOADD = splites[12]; FModel.ZIPL = splites[13]; FModel.ZIPR = splites[14]; FModel.FEATCAT = splites[15]; FModel.HYDROFLG = splites[16]; FModel.RAILFLG = splites[17]; FModel.ROADFLG = splites[18]; FModel.OLFFLG = splites[19]; FModel.PASSFLG = splites[20]; FModel.DIVROAD = splites[21]; FModel.EXTTYP = splites[22]; FModel.TTYP = splites[23]; FModel.DECKEDROAD = splites[24]; FModel.ARTPATH = splites[25]; FModel.PERSIST = splites[26]; FModel.GCSEFLG = splites[27]; FModel.OFFSETL = splites[28]; FModel.OFFSETR = splites[29]; FModel.TNIDF = splites[30]; FModel.TNIDT = splites[31];
            return FModel;
        }

        static object obj = new object();
        static void Log(string txt) {
            lock (obj) {
                //Append(@"E:\3-测试数据\Tiger\EDGES.csv\log.txt", txt + "\r\n");
            }
        }
        static void Log(string path, string txt) {
            lock (obj) {
                Append(path, txt + "\r\n");
            }
        }
        public static void Create(string path) {
            try {
                System.IO.FileStream fs = new System.IO.FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
            }
            catch {
                throw;
            }
        }

        public static void Append(string path, string text) {
            try {
                if (!File.Exists(path)) {
                    Create(path);
                }
                if (text == null)
                    return;
                StreamWriter sw = File.AppendText(path);
                sw.Write(text);
                sw.Close();
            }
            catch {
                throw;
            }
        }



        static void createIndex() {
            const string HOST = "localhost";
            const int PORT = 5432;
            const string USER = "postgres";
            const string PASSWORD = "admin";

            const string DB = "postgis_rcl";
            // 主表名称
            const string TABLE_NAME = "water";
            // 每个分区存储的最大记录数
            const int MAX_RECORD_COUNT = 100000;

            string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

            string line = string.Empty;
            int iXH = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"E:\3-测试数据\Tiger\AREAWATER.csv\AREAWATER.csv");
            while ((line = file.ReadLine()) != null) {
                //这里的Line就是您要的的数据了
                iXH++;//计数,总共几行
                int page = iXH / MAX_RECORD_COUNT;
                if (iXH % MAX_RECORD_COUNT == 1) {
                    using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                        try {
                            conn.Open();
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                            Console.Read();
                            return;
                        }
                        string subTableName = TABLE_NAME + "_" + page.ToString();
                        //string sqlIndex = string.Format("create  index idx_{0}_geom_{1} on {2} using gist(geom)", TABLE_NAME, page, subTableName);
                        string sqlIndex = string.Format("VACUUM {0}", subTableName);
                        //string sqlIndex = string.Format("drop index idx_{0}_geom_{1}", TABLE_NAME, page);
                        // 建立索引
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlIndex, conn)) {
                            try {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex) {
                                Console.WriteLine("create partion table {0}_{1} index error:\r\n{2}", TABLE_NAME, page, ex.ToString());
                                Console.Read();
                            }
                        }
                    }
                }
            }
        }

        static void import2() {
            const string HOST = "localhost";
            const int PORT = 5432;
            const string USER = "postgres";
            const string PASSWORD = "admin";

            const string DB = "postgis_rcl";
            // 主表名称
            const string TABLE_NAME = "water";
            // 每个分区存储的最大记录数
            const int MAX_RECORD_COUNT = 100000;

            string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                try {
                    conn.Open();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                    Console.Read();
                    return;
                }

                // 创建基础表
                using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE TABLE " + TABLE_NAME + "(id int)", conn)) {
                    try {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("CREATE TABLE " + TABLE_NAME + "(id int)");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("create table water error");
                        Console.Read();
                    }
                }
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)", conn)) {
                    try {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("add geom to table water error" + ex.StackTrace);
                        Console.Read();
                    }
                }




                string line = string.Empty;
                int iXH = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                System.IO.StreamReader file = new System.IO.StreamReader(@"E:\3-测试数据\Tiger\AREAWATER.csv\AREAWATER.csv");
                while ((line = file.ReadLine()) != null) {
                    //这里的Line就是您要的的数据了
                    iXH++;//计数,总共几行
                    int page = iXH / MAX_RECORD_COUNT;
                    if (iXH % MAX_RECORD_COUNT == 1) {
                        string sqlPartion = string.Format("CREATE TABLE {0}_{1}() INHERITS ({2})", TABLE_NAME, (int)(iXH / MAX_RECORD_COUNT), TABLE_NAME);
                        // 建立分区表
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlPartion, conn)) {
                            try {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex) {
                                Console.WriteLine("create partion table {0}_{1} error:\r\n{2}", TABLE_NAME, (int)(iXH / MAX_RECORD_COUNT), ex.ToString());
                                Console.Read();
                            }
                        }
                        string subTableName = TABLE_NAME + "_" + ((int)(iXH / MAX_RECORD_COUNT)).ToString();
                        string sqlRule = string.Format(@"CREATE OR REPLACE RULE insert_{0}_{1} 
AS ON INSERT TO {2} 
       WHERE id >= {3} AND id < {4}
       DO INSTEAD
       INSERT INTO {5} VALUES(NEW.*)", TABLE_NAME, (int)(iXH / MAX_RECORD_COUNT), TABLE_NAME, iXH, iXH + MAX_RECORD_COUNT, subTableName);

                        // 建立规则
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlRule, conn)) {
                            try {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex) {
                                Console.WriteLine(sqlRule);
                                Console.WriteLine("add partion table {0}_{1} rule error:\r\n{2}", TABLE_NAME, (int)(iXH / MAX_RECORD_COUNT), ex.StackTrace);
                                Console.Read();
                            }
                        }

                        string sqlIndex = string.Format("create index idx_{0}_geom_{1} on {2} using gist(geom)", TABLE_NAME, page, TABLE_NAME);
                        // 建立索引
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlIndex, conn)) {
                            try {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex) {
                                Console.WriteLine("create partion table {0}_{1} index error:\r\n{2}", TABLE_NAME, page, ex.ToString());
                                Console.Read();
                            }
                        }
                    }

                    // 插入数据
                    string sql = string.Format("INSERT INTO {0} (id, geom) VALUES ({1},ST_GeomFromText('{2}',4326))",
                        TABLE_NAME, iXH, line.Split(new char[] { '	' })[0].Trim('"'));

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) {
                        try {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) {
                            Console.WriteLine("{0} line error", iXH);

                        }
                    }

                    if (iXH % 10000 == 0) {
                        Console.WriteLine("processing 1 - {0}", iXH);
                    }

                    //if (iXH == 600000)
                    //{
                    //    break;
                    //}

                }

                sw.Stop();
                Console.WriteLine("used time:{0} scond", sw.ElapsedMilliseconds / 1000);
                file.Close();//关闭文件读取流

                conn.Close();
            }
        }

        static void import3() {
            //const string HOST = "192.168.1.201";
            const string HOST = "192.168.206.100";
            const int PORT = 5432;
            const string USER = "gpadmin";
            const string PASSWORD = "";

            const string DB = "gis_db";

            // 主表名称
            const string TABLE_NAME = "water";

            string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                try {
                    conn.Open();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                    return;
                }

                //// 创建基础表
                //using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE TABLE " + TABLE_NAME + "(id int)", conn))
                //{
                //    try
                //    {
                //        cmd.ExecuteNonQuery();
                //        Console.WriteLine("CREATE TABLE " + TABLE_NAME + "(id int)");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("create table water error");
                //        Console.Read();
                //    }
                //}
                //using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)", conn))
                //{
                //    try
                //    {
                //        cmd.ExecuteNonQuery();
                //        Console.WriteLine("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("add geom to table water error"+ex.StackTrace);
                //        Console.Read();
                //    }
                //}

                string line = string.Empty;
                int iXH = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //System.IO.StreamReader file = new System.IO.StreamReader(@"E:\3-测试数据\Tiger\AREAWATER.csv\AREAWATER.csv");
                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\data\AREAWATER.csv");
                using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    NpgsqlTransaction trans = conn.BeginTransaction();
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                    while ((line = file.ReadLine()) != null) {
                        //这里的Line就是您要的的数据了
                        iXH++;//计数,总共几行
                        string sql = string.Format("INSERT INTO water (id, geom) VALUES ({0},ST_GeomFromText('{1}',4326))",
                            iXH, line.Split(new char[] { '	' })[0].Trim('"'));
                        try {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) {
                            Console.WriteLine("{0} line error", iXH);
                        }

                        if (iXH % 10000 == 0) {
                            trans.Commit();
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                            Console.WriteLine("processing 1 - {0}", iXH);
                            trans = conn.BeginTransaction();

                        }
                        //if (iXH == 600000)
                        //{
                        //    break;
                        //}
                    }
                }
                sw.Stop();
                Console.WriteLine("used time:{0} scond", sw.ElapsedMilliseconds / 1000);
                file.Close();//关闭文件读取流

                conn.Close();
            }
        }

        static void importedge() {
            const string HOST = "192.168.1.201";
            const int PORT = 5432;
            const string USER = "gpadmin";
            const string PASSWORD = "";

            const string DB = "postgis_rcl";

            // 主表名称
            const string TABLE_NAME = "edge";

            string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                try {
                    conn.Open();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                    return;
                }

                //// 创建基础表
                //using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE TABLE " + TABLE_NAME + "(id int)", conn))
                //{
                //    try
                //    {
                //        cmd.ExecuteNonQuery();
                //        Console.WriteLine("CREATE TABLE " + TABLE_NAME + "(id int)");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("create table water error");
                //        Console.Read();
                //    }
                //}
                //using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)", conn))
                //{
                //    try
                //    {
                //        cmd.ExecuteNonQuery();
                //        Console.WriteLine("SELECT AddGeometryColumn ('water', 'geom', 4326, 'POLYGON', 2)");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("add geom to table water error"+ex.StackTrace);
                //        Console.Read();
                //    }
                //}

                string line = string.Empty;
                int iXH = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                System.IO.StreamReader file = new System.IO.StreamReader(@"E:\3-测试数据\Tiger\EDGES.csv\EDGES.csv");
                using (NpgsqlCommand cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;
                    NpgsqlTransaction trans = conn.BeginTransaction();
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                    while ((line = file.ReadLine()) != null) {
                        //这里的Line就是您要的的数据了
                        iXH++;//计数,总共几行
                        string sql = string.Format("INSERT INTO " + TABLE_NAME + " (id, geom) VALUES ({0},ST_GeomFromText('{1}',4326))",
                            iXH, line.Split(new char[] { '	' })[0].Trim('"'));
                        try {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) {
                            Console.WriteLine("{0} line error", iXH);
                        }

                        if (iXH % 10000 == 0) {
                            trans.Commit();
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                            Console.WriteLine("processing 1 - {0}", iXH);
                            trans = conn.BeginTransaction();

                        }
                        //if (iXH == 600000)
                        //{
                        //    break;
                        //}
                    }
                    trans.Commit();
                }
                sw.Stop();
                Console.WriteLine("used time:{0} scond", sw.ElapsedMilliseconds / 1000);
                file.Close();//关闭文件读取流

                conn.Close();
            }
        }

        static void import1() {
            const string HOST = "localhost";
            const int PORT = 5432;
            const string USER = "postgres";
            const string PASSWORD = "admin";

            const string DB = "postgis_rcl";

            string connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};CommandTimeout=0;",
                HOST, PORT, USER, PASSWORD, DB);

            using (NpgsqlConnection conn = new NpgsqlConnection(connString)) {
                try {
                    conn.Open();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                    return;
                }

                //CreateTable(conn);

                //NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, ST_AsText(geom), ST_AsEwkt(geom), ST_X(geom), ST_Y(geom) FROM cities", conn);

                //IDataReader dr = cmd.ExecuteReader();
                //while (dr.Read())
                //{
                //    for (int i = 0; i < dr.FieldCount; i++)
                //    {
                //        Console.Write(dr.GetValue(i));
                //        Console.Write("  |  ");
                //    }
                //    Console.WriteLine();
                //}
                //dr.Close();

                string line = string.Empty;
                int iXH = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                System.IO.StreamReader file = new System.IO.StreamReader(@"E:\3-测试数据\Tiger\EDGES.csv\EDGES.csv");
                while ((line = file.ReadLine()) != null) {
                    //这里的Line就是您要的的数据了
                    iXH++;//计数,总共几行
                    string sql = string.Format("INSERT INTO edge (id, geom) VALUES ({0},ST_GeomFromText('{1}',4326))",
                        iXH, line.Split(new char[] { '	' })[0].Trim('"'));

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn)) {
                        try {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) {
                            Console.WriteLine("{0} line error", iXH);
                        }
                    }

                    if (iXH % 10000 == 0) {
                        Console.WriteLine("processing 1 - {0}", iXH);
                    }
                    //if (iXH == 600000)
                    //{
                    //    break;
                    //}
                }

                sw.Stop();
                Console.WriteLine("used time:{0} scond", sw.ElapsedMilliseconds / 1000);
                file.Close();//关闭文件读取流

                conn.Close();
            }
        }


    }

    public class FLine_EDGES {
        public string WKT { get; set; }
        public string STATEFP { get; set; }
        public string COUNTYFP { get; set; }
        public string TLID { get; set; }
        public string TFIDL { get; set; }
        public string TFIDR { get; set; }
        public string MTFCC { get; set; }
        public string FULLNAME { get; set; }
        public string SMID { get; set; }
        public string LFROMADD { get; set; }
        public string LTOADD { get; set; }
        public string RFROMADD { get; set; }
        public string RTOADD { get; set; }
        public string ZIPL { get; set; }
        public string ZIPR { get; set; }
        public string FEATCAT { get; set; }
        public string HYDROFLG { get; set; }
        public string RAILFLG { get; set; }
        public string ROADFLG { get; set; }
        public string OLFFLG { get; set; }
        public string PASSFLG { get; set; }
        public string DIVROAD { get; set; }
        public string EXTTYP { get; set; }
        public string TTYP { get; set; }
        public string DECKEDROAD { get; set; }
        public string ARTPATH { get; set; }
        public string PERSIST { get; set; }
        public string GCSEFLG { get; set; }
        public string OFFSETL { get; set; }
        public string OFFSETR { get; set; }
        public string TNIDF { get; set; }
        public string TNIDT { get; set; }
    }
}
