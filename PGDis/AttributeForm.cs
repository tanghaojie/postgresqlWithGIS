using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PGDis {
    public partial class AttributeForm : Form {
        public AttributeForm(DataTable dt) {
            InitializeComponent();
            DataTable newDt = new DataTable();
            foreach (DataColumn item in dt.Columns) {
                newDt.Columns.Add(item.ColumnName);
            }
            int len = dt.Rows.Count;
            int cLen = newDt.Columns.Count;
            for (int i = 0; i < len; i++) {
                if (i > 50) {
                    break;
                }
                DataRow row = newDt.NewRow();
                for (int j = 0; j < cLen; j++) {
                    string cName = newDt.Columns[j].ColumnName;
                    if (dt.Columns.Contains(cName)) {
                        row[cName] = dt.Rows[i][cName];
                    }
                }
                newDt.Rows.Add(row);
            }
            dataGridView1.DataSource = newDt;
        }
    }
}
