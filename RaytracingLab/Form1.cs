using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaytracingLab {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        View view;

        private void Form1_Load(object sender, EventArgs e) {
            view = new View();
            view.setupView(glControl1.Width, glControl1.Height);
            //glControl1.Invalidate();
            Application.Idle += Application_Idle;
            //glControl1.Invalidate();
        }

        private void Application_Idle(object sender, EventArgs e) {
            while (glControl1.IsIdle) {
                //displayFPS();
                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e) {
            view.Draw();
            glControl1.SwapBuffers();
        }

        private void Form1_Resize(object sender, EventArgs e) {
            view.Resize(glControl1.Width, glControl1.Height);
        }
    }
}
