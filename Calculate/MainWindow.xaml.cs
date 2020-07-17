using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculate
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Label> varlabels = new List<Label>(); //存放面板上的变量
        List<Slider> varSliders = new List<Slider>(); //存放面板上的变量

        public static List<Label> expLabels = new List<Label>(); //存放面板上的表达式

        Expressions exp;
        string textStr;

        public MainWindow()
        {
            InitializeComponent();
            exp = new Expressions();
            text.Text = "a * sin ( b * x + log(2,sin(a)+3x*y) ^ log(2,sin(a))) + abs(3+x) * cos( cos(g)+(b-4ac) )";//
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textStr = text.Text.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");  //去除空格

            //MessageBox.Show(textStr);
            if (Expressions.VarOrExp(textStr) == 1) //变量
            {
                Expressions.AddOrChaVar(textStr);
                text.Text = "";
            }
            else  //表达式
            {
                if (exp.CheckIsExp(textStr))
                {

                    Expressions expression = new Expressions(textStr);

                    string str = expression.expression; //得到处理后的表达式
                    double result = expression.Calculate(); //计算结果

                    Creat_Exp_Label(str, result.ToString());

                    text.Text = "";
                }
            }

            Creat(Expressions.variables);
        }

        private void Creat(List<Variable> variables)  //动态生成未知数label
        {
            if (varlabels.Count != variables.Count) //如果这个未知数没有创建Label
            {
                for (int i = varlabels.Count; i < variables.Count; i++)
                {
                    Creat_Vari_Label(variables[i].name, variables[i].value.ToString());
                }
            }
            else
            {
                if (textStr.Contains("="))  //变量Label已经存在的
                {
                    string[] str = textStr.Split('=');
                    int n = variables.IndexOf(new Variable(str[0]));

                    double t = Expressions.variables[n].value;
                    varSliders[n].Value = t;

                    varlabels[n].Content = variables[n].name + "=" + variables[n].value;

                    for (int i = 0; i < variables[n].expressions.Count; i++)
                    {
                        double result = exp.Cal(exp.AddSub(exp.Division(variables[n].expressions[i]))); //将表达式重新计算
                        int m = variables[n].expIndex[i]; //找到这个表达式在label上的索引
                        expLabels[m].Content = variables[n].expressions[i] + "=" + result.ToString();
                    }
                }
            }
        }

        public void Drag_Slider(string varName, double value)//拖动滑动条的时候 
        {
            int n = Expressions.variables.IndexOf(new Variable(varName));  //这个变量在变量list的位置
            varlabels[n].Content = varName + "=" + value;
            Expressions.variables[n].value = value;
            for (int i = 0; i < Expressions.variables[n].expressions.Count; i++)//包含这个变量的表达式
            {
                double result = exp.Cal(exp.AddSub(exp.Division(Expressions.variables[n].expressions[i]))); //将表达式重新计算
                int m = Expressions.variables[n].expIndex[i]; //找到这个表达式在label上的索引
                expLabels[m].Content = Expressions.variables[n].expressions[i] + "=" + result.ToString();
            }
        }
        private void Creat_Vari_Label(string vari, string value)  //生成变量label
        {
            Label label = new Label
            {
                Name = vari,
                Height = 26,
                Width = 200
            };

            int hight = 55 * varlabels.Count - 410;
            label.Margin = new Thickness(550, hight, 0, 0);

            label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F8FF"));
            label.Content = vari + "=" + value;

            //鼠标拖动label
            TranslateTransform translateTransform = new TranslateTransform();
            label.RenderTransform = translateTransform;

            label.MouseLeftButtonDown += Grid_MouseLeftLabelDown;
            label.MouseMove += Grid_MouseMove;
            label.MouseLeftButtonUp += Grid_MouseLeftLabelUp;

            label.MouseRightButtonDown += Label_MouseRightButtonDown;

            varlabels.Add(label);
            Cal.Children.Add(label);

            //添加滑动改变变量
            Slider slider = new Slider
            {
                Name = vari,
                Style = (Style)this.FindResource("SliderStyle2"),
                RenderTransform = translateTransform
            };

            slider.MouseLeftButtonDown += Grid_MouseLeftLabelDown;
            slider.MouseMove += Grid_MouseMove;
            slider.MouseLeftButtonUp += Grid_MouseLeftLabelUp;

            slider.Height = 10;
            slider.Width = 200;
            slider.Value = Convert.ToDouble(value);

            hight = 55 * (varlabels.Count - 1) - 390;
            slider.Margin = new Thickness(550, hight, 0, 0);
            slider.ValueChanged += Sli_ValueChanged;

            varSliders.Add(slider);
            Cal.Children.Add(slider);
        }

        private void Label_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;

            int n = Expressions.variables.IndexOf(new Variable(label.Name));
            Window2 window2 = new Window2(Expressions.variables[n]);
            window2.ShowDialog();

            Expressions.variables[n] = window2.vari();

            varSliders[n].Minimum = Expressions.variables[n].min;
            varSliders[n].Maximum = Expressions.variables[n].max;
            double t = Expressions.variables[n].value;
            varSliders[n].Value = t;

            //MessageBox.Show(varSliders[n].Value.ToString());

            varlabels[n].Content = label.Name + "=" + Expressions.variables[n].value;



            for (int i = 0; i < Expressions.variables[n].expressions.Count; i++)
            {
                double result = exp.Cal(exp.AddSub(exp.Division(Expressions.variables[n].expressions[i]))); //将表达式重新计算
                int m = Expressions.variables[n].expIndex[i]; //找到这个表达式在label上的索引
                expLabels[m].Content = Expressions.variables[n].expressions[i] + "=" + result.ToString();
            }
        }



        private void Creat_Exp_Label(string exp, string value)  //生成表达式label
        {
            Label label = new Label
            {
                Height = 26,
                Width = 550,
            };
            int hight = 55 * expLabels.Count - 410;

            label.Margin = new Thickness(-210, hight, 0, 0);
            label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F8FF"));

            label.Content = exp + "=" + value;
            //鼠标拖动label
            TranslateTransform translateTransform = new TranslateTransform();
            label.RenderTransform = translateTransform;
            label.MouseLeftButtonDown += Grid_MouseLeftLabelDown;
            label.MouseMove += Grid_MouseMove;
            label.MouseLeftButtonUp += Grid_MouseLeftLabelUp;

            expLabels.Add(label); //

            Cal.Children.Add(label);
        }



        #region 拖动label
        private bool isDragging;
        private Point startPosition;

        private void Grid_MouseLeftLabelDown(object sender, MouseButtonEventArgs e)
        {

            isDragging = true;
            var draggableElement = sender as UIElement;
            var clickPosition = e.GetPosition(this);

            var transform = draggableElement.RenderTransform as TranslateTransform;

            startPosition.X = clickPosition.X - transform.X;    //注意减号
            startPosition.Y = clickPosition.Y - transform.Y;

            draggableElement.CaptureMouse();
        }

        private void Grid_MouseLeftLabelUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var draggableElement = sender as UIElement;
            draggableElement.ReleaseMouseCapture();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && sender is UIElement draggableElement)
            {
                Point currentPosition = e.GetPosition(this.Parent as UIElement);
                var transform = draggableElement.RenderTransform as TranslateTransform;

                transform.X = currentPosition.X - startPosition.X;
                transform.Y = currentPosition.Y - startPosition.Y;
            }
        }

        #endregion

        private void Sli_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            Drag_Slider(slider.Name, e.NewValue);
        }
    }


    /// <summary>
    /// 表达式处理
    /// </summary>
    public class Expressions
    {
        public string expression;
        public List<string> expList;

        #region 构造函数
        public Expressions()
        {

        }
        public Expressions(string expression)
        {
            expList = new List<string>();
            this.expression = expression;
            CheckIsExp(expression);
            Exp_Pretreat();
            //IsExpression(expression);
        }
        #endregion

        public static List<Variable> variables = new List<Variable>(); //存放所有表达式的变量

        public string symbol = "+-*/^(){}[],"; //合法符号和表达式
        public string number = "1234567890."; //组成常数的数字
        public string UnRight = "（）【】。"; //不合法的符号
        #region 检查表达式括号是否正确
        public static bool IsBracketMatch(string expression) //表达式括号是否匹配
        {
            //待解决：中文状态下输入括号
            string str = "";

            Stack<char> stack = new Stack<char>();
            foreach (char ch in expression)
            {
                //MessageBox.Show(expression);

                str += ch.ToString();

                if (ch == '(' || ch == '[' || ch == '{')
                {
                    stack.Push(ch);
                }
                else if (ch == ')' || ch == ']' || ch == '}')
                {
                    if (stack.Count == 0)
                    {
                        //MessageBox.Show(str);

                        return false;

                    }

                    if (Match(stack.Pop(), ch) == false)
                        return false;
                }
            }

            if (stack.Count == 0)
                return true;
            else
                return false;
        }

        public static bool Match(char ch1, char ch2)
        {
            if (ch1 == '(' && ch2 == ')' || ch1 == '[' && ch2 == ']' || ch1 == '{' && ch2 == '}')
                return true;
            else
                return false;
        }

        #endregion

        public static int VarOrExp(string str) //变量还是表达式,1:变量。2:表达式
        {
            if (str.Contains("="))
            {
                return 1;
            }
            return 2;
        }

        public static bool AddOrChaVar(string str) //将变量添加到
        {

            string[] str1 = str.Split('=');

            if (str1[0][0] >= 'a' && str1[0][0] <= 'z' || str1[0][0] >= 'A' && str1[0][0] <= 'Z')
            {
                try
                {
                    double temp = Convert.ToDouble(str1[1]);

                    Variable variable = new Variable(str1[0], temp);

                    if (Expressions.variables.Contains(variable))  //如果这个变量存在（改变变量的值
                    {
                        int n = Expressions.variables.IndexOf(variable);
                        Expressions.variables[n].value = temp;
                        return true;
                    }
                    else
                    {
                        Expressions.variables.Add(variable); //如果变量不存在，创建
                        return true;
                    }
                }
                catch
                {

                    MessageBox.Show("输入的变量数值格式不正确");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("输入的变量名称格式不正确");
                return false;
            }
        }


        #region 检查表达式
        //检查一个string(可能是变量、数字）是否为数字
        public bool IsNumber(string str)
        {
            return Regex.Matches(str, "[0-9.]").Count > 0 && Regex.Matches(str, "[a-zA-Z]").Count == 0;
        }

        public bool IsExpression(string expression)  //判断是否为合法表达式
        {

            if (expression.Length == 0) //表达式为空
            {
                MessageBox.Show("表达式为空");
                return false;
            }

            foreach (char ch in expression)
            {
                if (UnRight.Contains(Convert.ToString(ch)))
                {
                    MessageBox.Show("请在英文状态下输入符号，请重新输入");
                    return false;
                }
            }

            if (IsBracketMatch(expression) == false)  //括号sh
            {
                //MessageBox.Show(expression);

                MessageBox.Show("括号不匹配，请重新输入11");
                return false;
            }

            string[] str1 = { "+-*/^", "*/^" };
            string[] str2 = { "([{", ")]}" };

            if (str1[0].Contains(expression[expression.Length - 1]) || str1[1].Contains(expression[0]))
            {
                MessageBox.Show("运算符缺少操作数");
                return false;
            }

            //a*sin(b*x+c^log(2,sin(a)))+abs(3+x)*cos(cos(b)+(b-4ac))/(2floor(x))-log(a,x)
            for (int i = 1; i < expression.Length; i++)  //连续出现运算符
            {
                if ((str1[0].Contains(expression[i - 1]) && (str1[0].Contains(expression[i]) || str2[1].Contains(expression[i]) || expression[i] == ','))
                    || (expression[i - 1] == ',' && (str1[1].Contains(expression[i]) || str2[1].Contains(expression[i])))
                    || (str2[0].Contains(expression[i - 1]) && (str1[1].Contains(expression[i]) || expression[i] == ','))
                    || (str2[0].Contains(expression[i - 1]) && str2[1].Contains(expression[i])))
                {
                    MessageBox.Show(String.Format("{0}运算符缺少操作数", expression[i - 1]));
                    return false;
                }
            }
            return true;
        }

        public List<string> Division(string expression)  //将表达式分割多个小部分
        {
            List<string> strList = new List<string>();

            int current = 0;
            int preType = 1;  // 上一个字符的类型，1运算符号，2字母，3数字（包括小数点）

            while (current < expression.Length)
            {
                if (symbol.Contains(expression[current])) //符号
                {
                    strList.Add(expression[current].ToString());
                    preType = 1;
                }
                else if (number.Contains(expression[current]) && preType != 2) //数字
                {
                    if (strList.Count > 0)
                    {
                        if (preType != 3)
                            strList.Add(expression[current].ToString());
                        else
                            strList[strList.Count - 1] += expression[current];
                    }
                    else
                        strList.Add(expression[current].ToString());

                    preType = 3;
                }
                else //字母
                {

                    if (preType != 2)
                        strList.Add(expression[current].ToString());
                    else
                    {
                        strList[strList.Count - 1] += expression[current];
                    }

                    preType = 2;
                }
                current++;

            }
            return strList;
        }

        //检查表达式的常量和函数的参数是否正确
        public bool Check(List<string> strList)
        {

            foreach (string str in strList)  //检查输入的常量是否正确
            {
                if (Regex.Matches(str, "[0-9.]").Count > 0 && Regex.Matches(str, "[a-zA-Z]").Count == 0)
                {
                    try
                    {
                        Convert.ToDouble(str);
                    }
                    catch
                    {
                        MessageBox.Show(String.Format("表达式中:{0} 是错误的常量", str));
                        return false;
                    }
                }
            }

            //检查表达式函数的参数个数是否正确
            for (int i = 0; i < strList.Count; i++)
            {
                string[] str = { "([{", ")]}" };

                if (Function.m_Operators.Contains(strList[i])) //单目运算符
                {
                    if (strList[i + 1] != "(")
                    {
                        MessageBox.Show(String.Format("表达式中:{0} 缺少操作数", strList[i]));
                        return false;
                    }

                    int count = 0;
                    string str1 = "";
                    for (int j = i + 1; j < strList.Count - 1; j++)
                    {
                        if (str[0].Contains(strList[j]))
                            count++;
                        else if (str[1].Contains(strList[j]))
                            count--;

                        str1 += strList[j];

                        //MessageBox.Show(str1);
                        if (count == 0)
                        {
                            i = j;
                            break;
                        }

                        if (count == 1 && strList[j + 1] == "=")
                        {
                            if (!IsExpression(str1))
                            {
                                MessageBox.Show(String.Format("函数:{0}的参数错误。", strList[i]));
                                return false;
                            }
                        }
                    }
                }
                else if (Function.dou_Operators.Contains(strList[i]) && strList[i] != "^") //双目
                {
                    if (strList[i + 1] != "(")
                    {
                        MessageBox.Show(String.Format("表达式中:{0} 缺少操作数", strList[i]));
                        return false;
                    }

                    int n = 0;
                    int count = 0;
                    string str1 = "";
                    for (int j = i + 2; j < strList.Count - 1; j++)
                    {
                        if (str[0].Contains(strList[j]))
                            count++;
                        else if (str[1].Contains(strList[j]))
                            count--;
                        str1 += strList[j];

                        if (count == 0 && strList[j + 1] == ",") //函数第一个操作数
                        {
                            if (!IsExpression(str1))
                            {
                                MessageBox.Show(String.Format("函数{0}第一个操作数错误", strList[i]));
                                return false;
                            }
                            else
                            {
                                n = j + 1;
                                break;
                            }
                        }
                        else if (count == 0 && strList[j + 1] == ")")
                        {
                            MessageBox.Show(String.Format("函数{0}缺少一个操作数", strList[i]));
                            return false;
                        }
                    }

                    //////////////////////// sin(a))) + abs(3+x)
                    count = 0;
                    str1 = "";

                    for (int j = n + 1; j < strList.Count; j++) //函数第二个操作数
                    {
                        if (str[0].Contains(strList[j]))
                            count++;
                        else if (str[1].Contains(strList[j]))
                            count--;
                        str1 += strList[j];

                        if (count == 0 && strList[j + 1] == ")")
                        {
                            //MessageBox.Show(str1);
                            if (!IsExpression(str1))
                            {
                                MessageBox.Show(String.Format("函数{0}第二个操作数错误", strList[j]));
                                return false;
                            }
                            else
                            {
                                MessageBox.Show(str1);
                                return true;
                            }
                        }

                    }

                }
            }
            return true;
        }

        //检查表达式是否正确
        public bool CheckIsExp(string str)
        {
            if (IsExpression(str))
            {
                //MessageBox.Show("999");

                if (Check(Division(str)))
                    return true;
                else
                {
                    //MessageBox.Show(str);

                    return false;
                }
            }
            else
                return false;
        }

        #endregion


        public void AddVar(List<string> strList)   //得到变量的值
        {
            Random random = new Random();

            for (int i = 0; i < strList.Count; i++)
            {
                if (Regex.Matches(strList[i], "[a-zA-Z]").Count > 0
                    && !Function.m_Operators.Contains(strList[i])
                    && !Function.dou_Operators.Contains(strList[i]))
                {
                    Variable variable = new Variable(strList[i], random.NextDouble() * 10);
                    if (!variables.Contains(variable))
                    {
                        variables.Add(variable);
                    }

                    if (!variables[variables.IndexOf(variable)].expressions.Contains(expression))
                    {
                        variables[variables.IndexOf(variable)].expressions.Add(expression); //在变量添加表达式

                        variables[variables.IndexOf(variable)].expIndex.Add(MainWindow.expLabels.Count); //在变量添加表达式的label的索引
                    }
                }
            }
        }

        public List<string> AddMul(List<string> strrList) //在数字和字母之间添加乘号
        {
            List<string> strList1 = new List<string>
            {
                strrList[0]
            };

            for (int i = 1; i < strrList.Count; i++)
            {
                if ((double.TryParse(strrList[i - 1], out _) || strrList[i - 1] == ")") && Regex.Matches(strrList[i], "[a-zA-Z(]").Count > 0)
                {
                    strList1.Add("*");
                }
                strList1.Add(strrList[i]);
            }
            return strList1;
        }

        public List<string> AddSub(List<string> strList) //在一个数的相反数如-x补上省略的0得到0-x
        {
            List<string> strList1 = new List<string>();

            if (strList[0] == "-" || strList[0] == "+")
            {
                strList1.Add("0");
                strList1.Add(strList[0]);
            }
            else
                strList1.Add(strList[0]);

            for (int i = 1; i < strList.Count; i++)
            {
                if (strList[i - 1] == "(" && (strList[i] == "-" || strList[i] == "+"))
                    strList1.Add("0");

                strList1.Add(strList[i]);
            }
            return strList1;
        }


        public string AddList(List<string> vs)  //将表达式list组装成string//显示
        {
            string str = "";
            foreach (string str1 in vs)
            {
                str += str1;
            }
            return str;
        }


        public string Exp_Pretreat()
        {
            expList = AddMul(Division(expression)); //将表达式分割、添加乘号

            expression = AddList(expList);

            AddVar(expList);

            expList = AddSub(expList); //负号前添加0

            return expression;
        }

        public double Cal(List<string> exp)  //计算表达式 返回计算结果
        {
            List<string> symbols = new List<string>(); //记录每个item之间的运算符

            string[] brackets = { "([{", ")]}" };  //括号

            List<string> ste = new List<string>();  //组成一个item的字符

            List<Item> items = new List<Item>();   //表达式的每个item

            int count = 0;
            for (int i = 0; i < exp.Count; i++)  //分割得到item
            {

                if (i == exp.Count - 1)
                {
                    ste.Add(exp[i]);

                    /* string stt = "";
                     foreach(string str in ste)
                     {
                         stt += str;
                     }
                     MessageBox.Show(stt);*/

                    items.Add(new Item(ste));

                }
                else if ((exp[i] == "+" || exp[i] == "-") && count == 0)
                {
                    /*string stt = "";
                    foreach (string str in ste)
                    {
                        stt += str;
                    }
                    MessageBox.Show(stt);*/

                    items.Add(new Item(ste));
                    symbols.Add(exp[i]);
                    ste = new List<string>();
                }
                else
                {
                    if (brackets[0].Contains(exp[i]))
                        count++;
                    else if (brackets[1].Contains(exp[i]))
                        count--;

                    ste.Add(exp[i]);
                }
            }

            //将item计算
            double sum = items[0].Func1();
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] == "-")
                    sum -= items[i + 1].Func1();
                else
                    sum += items[i + 1].Func1();

            }
            return sum;
        }

        public double Calculate()  //计算表达式 返回计算结果
        {
            return Cal(expList);
        }
    }

    /// <summary>
    /// 表达式每个子项
    /// </summary>
    public class Item
    {
        List<Factor> molecule; //分子

        List<Factor> denominator; //分母

        public List<string> itemList; //item

        public Item()
        {

        }

        public Item(List<string> str)
        {
            itemList = str;
            molecule = new List<Factor>(); //分子
            denominator = new List<Factor>(); // 分母
        }

        //没有求模
        public double Func1()  //计算每个item的的结果
        {
            int count = 0;//记录不匹配的括号的数量

            string[] brackets = { "([{", ")]}" }; //括号

            List<Factor> factors = new List<Factor>(); //临时存放

            List<string> symbols = new List<string>(); //存放运算符

            List<string> str = new List<string>();  //组成分子、分母的元素

            for (int i = 0; i < itemList.Count; i++)  //将一个item按照 * / 分割成一个个factor
            {
                if (i == itemList.Count - 1) //最后一项
                {
                    str.Add(itemList[i]);
                    factors.Add(new Factor(str));
                }
                else if (itemList[i] == "*" && count == 0) //分子
                {
                    factors.Add(new Factor(str));
                    str = new List<string>();
                    symbols.Add(itemList[i]);

                }
                else if (itemList[i] == "/" && count == 0) //分母
                {
                    factors.Add(new Factor(str));
                    str = new List<string>();
                    symbols.Add(itemList[i]);
                }
                else
                {
                    if (brackets[0].Contains(itemList[i]))
                        count++;
                    else if (brackets[1].Contains(itemList[i]))
                        count--;

                    str.Add(itemList[i]);
                }
            }

            molecule.Add(factors[0]); //第一项肯定是分子
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] == "*")
                {
                    molecule.Add(factors[i + 1]);

                }
                else if (symbols[i] == "/")
                {
                    denominator.Add(factors[i + 1]);
                }
            }

            double sum1 = 1;
            for (int i = 0; i < molecule.Count; i++) //计算分字的值
            {
                sum1 *= molecule[i].Func2();
            }

            double sum2 = 1;
            for (int i = 0; i < denominator.Count; i++) //计算分母的值
            {
                sum2 *= denominator[i].Func2();
            }

            double sum;
            if (sum2 == 0)
            {
                //MessageBox.Show("分母出现不能为0！");
                return double.NaN;
            }
            else
                sum = sum1 / sum2;

            return sum;
        }

        //有求模
        public double Func2()  //计算每个item的的结果
        {
            int count = 0;//记录不匹配的括号的数量

            string[] brackets = { "([{", ")]}" }; //括号

            List<Factor> factors = new List<Factor>(); //临时存放

            List<string> symbols = new List<string>(); //存放运算符

            List<string> str = new List<string>();  //组成分子、分母的元素

            for (int i = 0; i < itemList.Count; i++)  //将一个item按照 * / 分割成一个个factor
            {
                if (i == itemList.Count - 1) //最后一项
                {
                    str.Add(itemList[i]);
                    factors.Add(new Factor(str));
                }
                else if (itemList[i] == "*" && count == 0) //分子
                {
                    factors.Add(new Factor(str));
                    str = new List<string>();
                    symbols.Add(itemList[i]);

                }
                else if (itemList[i] == "/" && count == 0) //分母
                {
                    factors.Add(new Factor(str));
                    str = new List<string>();
                    symbols.Add(itemList[i]);

                }
                else if (itemList[i] == "%" && count == 0) //分母
                {
                    factors.Add(new Factor(str));
                    str = new List<string>();
                    symbols.Add(itemList[i]);

                }
                else
                {
                    if (brackets[0].Contains(itemList[i]))
                        count++;
                    else if (brackets[1].Contains(itemList[i]))
                        count--;

                    str.Add(itemList[i]);
                }
            }

            double sum = factors[0].Func2();

            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] == "*")
                {
                    sum *= factors[i + 1].Func2();
                }
                else if (symbols[i] == "/")
                {
                    if (factors[i + 1].Func2() == 0)
                    {
                        MessageBox.Show("分母不能为0！");
                        return double.NaN;
                    }
                    else
                        sum /= factors[i + 1].Func2();
                }
                else if (symbols[i] == "%")
                {
                    if (factors[i + 1].Func2() == 0)
                    {
                        MessageBox.Show("分母不能为0！");
                        return double.NaN;
                    }
                    else
                        sum %= factors[i + 1].Func2();
                }
            }
            return sum;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Factor
    {
        /* double con;
         Variable var;
         Expressions exp;
         Function Function;*/

        List<string> facList;

        public Factor()
        {

        }
        public Factor(List<string> str)
        {
            facList = str;
        }

        // sin ( x )
        //判断strList是不是单纯的函数 ^
        public int IsFunc(List<string> strList)
        {
            string[] str = { "([{", ")]}", "", "+-*/" };

            if (Function.dou_Operators.Contains(strList[0]) || Function.m_Operators.Contains(strList[0]))  //函数 //&& strList[0] != "^"
            {
                int count = 1;
                //string sttt = "";

                for (int i = 2; i < strList.Count; i++)
                {
                    //sttt += strList[i];
                    if (str[0].Contains(strList[i]))
                        count++;
                    else if (str[1].Contains(strList[i]))
                        count--;

                    //log(2,sin(a))^c
                    if (count == 0)
                    {
                        if (i == strList.Count - 1) //非^的函数
                            return 1;
                        else if (strList[i + 1] == "^") //可能是^的函数
                            break;
                        else   //是表达式
                            return 0;
                    }
                }
            }

            //a^b
            if (strList.Contains("^"))  //乘方
            {
                int count = 0;
                int n = 0;

                //( a * sin ( x ^ y )^y )    sin(x)^y
                //x^sin(y)
                //((x))
                for (int i = 0; i < strList.Count; i++)
                {
                    if (str[0].Contains(strList[i]))
                        count++;
                    else if (str[1].Contains(strList[i]))
                        count--;

                    if (count == 0)  // && i < strList.Count
                    {
                        if (Function.dou_Operators.Contains(strList[i]) || Function.m_Operators.Contains(strList[i])) //sin(x)^y || log(x,y)^y
                        {
                            continue;
                        }
                        else if (str[3].Contains(strList[i + 1])) //x+y^z
                        {
                            /*string str2 = "";
                            for(int j = i; j<strList.Count;j++)
                            {
                                str2 += strList[j];
                            }
                            MessageBox.Show(str2);*/
                            return 0;
                        }
                        else if (strList[i + 1] == "^")  //x^sin(y)
                        {
                            n = i + 1;
                            break;
                        }
                        else
                        {
                            string str2 = "";
                            foreach (string str1 in strList)
                            {
                                str2 += str1;
                            }
                            MessageBox.Show("未解决(IsFunc函数)" + str2);

                            return 0;
                        }
                    }
                }
                //MessageBox.Show(strList[n + 2]);

                //sin ( x ) ^ y  + 2
                //x^sin(y)
                for (int i = n + 1; i < strList.Count; i++)
                {
                    if (str[0].Contains(strList[i]))
                        count++;
                    else if (str[1].Contains(strList[i]))
                        count--;

                    if (count == 0)
                    {
                        if (i == strList.Count - 1)
                        {
                            return 2;
                        }
                        else if (Function.dou_Operators.Contains(strList[i]) || Function.m_Operators.Contains(strList[i]))
                        {
                            continue;
                        }
                        else if (str[3].Contains(strList[i + 1]))
                        {
                            return 0;
                        }
                    }
                }
            }

            return 0;
        }

        public double Func2()  //返回每个因子
        {
            double temp = 0;

            string[] str = { "([{", ")]}" };

            Expressions expressions = new Expressions();
            //MessageBox.Show(facList[0]);


            if (str[0].Contains(facList[0]))  //删除这一项外面的括号((x-y)*x) -> (x-y)*z
            {
                int count = 0;
                for (int i = 0; i < facList.Count; i++)
                {
                    if (str[0].Contains(facList[i]))
                        count++;
                    else if (str[1].Contains(facList[i]))
                        count--;

                    if (count == 0)
                    {
                        if (i == facList.Count - 1)
                        {
                            facList.RemoveAt(facList.Count - 1);
                            facList.RemoveAt(0);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (facList.Count == 1)  //如果只有一项，只能是变量或者常量
            {
                if (double.TryParse(facList[0], out temp)) //常量，直接将字符转化为double的数
                {
                    return temp;
                }
                else if (Expressions.variables.Contains(new Variable(facList[0]))) //变量
                {
                    int n = Expressions.variables.IndexOf(new Variable(facList[0])); //找到这个变量的索引
                    temp = Expressions.variables[n].value;

                    return temp;
                }
            }
            else  //表达式||函数
            {
                int tpm = IsFunc(facList);
                double number1; //第一个参数
                double number2; //第二个参数（双目运算

                List<string> exp1, exp2;

                if (tpm == 1) //函数
                {
                    if (Function.dou_Operators.Contains(facList[0])) //双目
                    {
                        //MessageBox.Show("双目");
                        int n = 0;
                        int count = 0;
                        exp1 = new List<string>();  //^前
                        for (int i = 2; i < facList.Count; i++)
                        {
                            if (str[0].Contains(facList[i]))
                                count++;
                            else if (str[1].Contains(facList[i]))
                                count--;
                            exp1.Add(facList[i]);

                            if (count == 0 && facList[i + 1] == ",")
                            {
                                n = i;
                                break;
                            }
                        }

                        exp2 = new List<string>(); //^后
                        for (int i = n + 2; i < facList.Count - 1; i++)
                            exp2.Add(facList[i]);


                        number1 = expressions.Cal(exp1);
                        number2 = expressions.Cal(exp2);

                        Function factor = new Function(facList[0], number1, number2);
                        temp = factor.Func();
                        return temp;

                    }
                    else if (Function.m_Operators.Contains(facList[0]))   //单目
                    {
                        //MessageBox.Show("单目");

                        exp1 = new List<string>();
                        string str2 = "";

                        for (int i = 2; i < facList.Count - 1; i++)
                        {
                            exp1.Add(facList[i]);
                            str2 += facList[i];

                        }


                        //MessageBox.Show(str2);

                        number1 = expressions.Cal(exp1);

                        Function factor = new Function(facList[0], number1);
                        temp = factor.Func();
                        return temp;
                    }

                }
                else if (tpm == 2)//处理乘方 ^ 
                {
                    //MessageBox.Show("liang");

                    int count = 0;
                    int n = 0;

                    //string stttt = "";
                    exp1 = new List<string>();  //^前
                    for (int i = 0; i < facList.Count; i++)
                    {
                        //stttt += facList[i];

                        if (str[0].Contains(facList[i]))
                            count++;
                        else if (str[1].Contains(facList[i]))
                            count--;
                        exp1.Add(facList[i]);
                        if (count == 0)
                        {
                            if (!Function.dou_Operators.Contains(facList[i]) && !Function.m_Operators.Contains(facList[i]))
                            {

                                n = i;
                                break;
                            }
                        }
                    }

                    //MessageBox.Show(stttt);
                    exp2 = new List<string>(); //^后
                    for (int i = n + 2; i < facList.Count; i++)
                    {
                        exp2.Add(facList[i]);
                        //stttt += facList[i];
                    }

                    number1 = expressions.Cal(exp1);
                    number2 = expressions.Cal(exp2);

                    Function factor = new Function("^", number1, number2);

                    temp = factor.Func();
                    return temp;

                }
                else  //表达式
                {
                    //MessageBox.Show("falssss");

                    if (facList.Count > 0)
                        temp = expressions.Cal(facList);

                    return temp;

                }
            }
            MessageBox.Show("error???");

            return temp;
        }
    }

    /// <summary>
    /// 变量
    /// </summary>
    public class Variable
    {
        public double value;   //变量的数值

        public double min;  //变量的最小值
        public double max;  //变量的最大值

        public string name;    // 变量名

        public List<int> expIndex; //拥有该变量的表达式的 label的索引
        public List<string> expressions; //拥有该变量的表达式

        public Variable()
        {

        }
        public Variable(string name)
        {
            this.name = name;
        }

        public Variable(string name, double value)
        {
            this.name = name;
            this.value = value;
            min = 0;
            max = 10;
            expressions = new List<string>();
            expIndex = new List<int>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(Variable))
                return false;
            Variable c = obj as Variable;
            return this.name == c.name;
        }

        public override int GetHashCode()  //不确定
        {
            return this.name.GetHashCode();
        }
    }

    /// <summary>
    /// 表达式中的函数
    /// </summary>
    public class Function
    {
        double number1;
        double number2;

        string symbol;

        public Function()
        {

        }
        public Function(string symbol, double num1)
        {
            this.symbol = symbol;
            number1 = num1;
        }

        public Function(string symbol, double num1, double num2)
        {
            this.symbol = symbol;
            number1 = num1;
            number2 = num2;
        }
        public static List<string> m_Operators = new List<string>(new string[] { "floor", "sqrt", "tan", "atan", "sin", "cos", "asin", "acos", "abs" });   //允许使用的单目运算符

        public static List<string> dou_Operators = new List<string>(new string[] { "pow", "^", "log", });   //允许使用的双目运算符

        public double Func()
        {
            switch (symbol)
            {
                case "sin":
                    return Math.Sin(number1);
                case "cos":
                    return Math.Cos(number1);
                case "tan":
                    return Math.Tan(number1);
                case "asin":
                    return Math.Asin(number1);
                case "acos":
                    return Math.Acos(number1);
                case "atan":
                    return Math.Atan(number1);
                case "abs":
                    return Math.Abs(number1);
                case "floor":
                    return Math.Floor(number1);

                case "pow":
                    return Math.Pow(number1, number2);
                case "^":
                    return Math.Pow(number1, number2);
                case "log":
                    return Math.Log(number1, number2);

            }
            return 0;
        }
    }
}


