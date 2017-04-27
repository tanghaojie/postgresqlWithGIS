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
    public partial class QueryForm : Form {
        public QueryForm() {
            InitializeComponent();
        }

        public string QueryTxt { get; set; }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e) {
            string txt = textBox1.Text;
            if (string.IsNullOrEmpty(txt)) {
                MessageBox.Show("缺少查询条件，请重新输入");
                return;
            }
            if (txt.Contains("'")) {
                MessageBox.Show("查询条件不能包含单引号[']，请重新输入");
                return;
            }
            QueryTxt = txt;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
