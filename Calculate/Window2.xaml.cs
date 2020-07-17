using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Calculate
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        Variable variable;
        double varMin;
        double varMax;
        double valu;

        public Window2()
        {
            InitializeComponent();
        }

        public Window2(Variable variable)
        {
            InitializeComponent();
            varName.Content = variable.name;
            value.Text = variable.value.ToString();
            min.Text = variable.min.ToString();
            max.Text = variable.max.ToString();

            this.variable = new Variable(variable.name);
            //this.variable.value = variable.value;
            this.variable = variable;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                valu = Convert.ToDouble(value.Text);
            }
            catch
            {
                MessageBox.Show("输入数值格式不正确,请重新输入");
            }
            try
            {
                varMin = Convert.ToDouble(min.Text);
            }
            catch
            {
                MessageBox.Show("输入的MIN数值格式不正确,请重新输入");
            }

            try
            {
                varMax = Convert.ToDouble(max.Text);
            }
            catch
            {
                MessageBox.Show("输入的MAX数值格式不正确,请重新输入");
            }
            if (varMin < varMax)
            {
                variable.value = valu;
                variable.min = varMin;
                variable.max = varMax;
                MessageBox.Show("修改成功。");
            }
            else
            {
                MessageBox.Show("min大于max了！");
            }


        }

        public Variable vari()
        {
            //MessageBox.Show("KK" + variable.value.ToString());

            return variable;
        }

    }
}
