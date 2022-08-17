using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WebSocket4Net;
using System.Security.Authentication;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace stockbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class StockSecond
    {
        public string ev { get; set; }
        public string sym { get; set; }
        public int[] i { get; set; }
        public int bx { get; set; }
        public int ax { get; set; }
        public float bp { get; set; }
        public float ap { get; set; }
        public int bs { get; set; }
        public int _as { get; set; }
        public double t { get; set; }
        public int q { get; set; }
        public int z { get; set; }

    }

    public partial class MainWindow : Window
    {
        private static HttpClient client = new HttpClient();
        string main_url = "https://api.tdameritrade.com/v1/";
        string account_id = "488801007";
        string client_id = "QJDAGL13M66A3K4O6LGYBUR0XH3YXJFE";
        string refresh_token = "2Pb/ftYvZ8QMN6REGhZkbL5QhoIkNegDIj25gX/ONOjHCfr42r+pS3q5jckaiYOQ26iekhVjL2ycrhsWS1Rf82004r5Rn0IJciRAMGaPtMpigJ/qSZTA5tffxYrF8DrcfxN3F6bFzd6/ymKaKpN4TrMbCFISK+0rxszFcq3uKFUvOAY3xQoXp5wm84IK1wJHV5M1DRr7iHKLeYw1iGzrxsWK1/FMa7nIeLfYo0Ume1P/AagoahU1GkZD7Pn4kY5iBNP1/meG1ednCAyAIyU9KMAE/91i7xg91klsohA93j78vhiRvd3Oy3pgZ8wulQJPLuxpPp6Xth84qBHL+8N8FWzGYiujdsH0yvHtldiNYaXAR3zbgE9Zu2u//Yv4kX+ResdiXhfpGdsGeyKTnR+ZGBKucbLwJeMRdL07AH+k5HDFbg7MG3UzTQDUGMF100MQuG4LYrgoVi/JHHvlDLskO6fv0DQ4v4TajvNs+/4Wk5bxWA2pj2bRzsVdbciYY3U19FVwB2KeqXF0/wytg1O4+Fqi4Vi776ugaY6GSaGlhCzHAvLtoa+BQqZatDkAyBOZpYW4QMGm9gvhKl7X+EQPm9mbHM1ZjD7YTeyDacz4D0fmSaBmGiBJbxAXWuxNzGN+ZgF1r+BjqB2Q2bufuntndUpvNAPP5eJMmFyOxUMiPFH0WWHR+EqAaF97ZaF2lAW3yxDEfNehsYSG6i/MufYtTtFyQ5xEItzLMXiBpI9svIH9TACcWHkvcN/slZpRjNPd5D0ItC9ezXlrlEPE+/MPO3lgr1vHb6g5rvNLGHuqBC65tlOYohuicV2AyN/wjKmHyk4M1Wa7eBnNyypZd21vxfkl9hKA7dLrv9EJHqfryNXrNDrEy9vQLZTc5RxUpeEuSuIlP0hTIBA=212FD3x19z9sWBHDJACbC00B75E";

        double amount_value = 0;
        Boolean automation_flag = false;
        string first_buy_sym = "";
        string second_buy_sym = "";
        
        int first_buy_qty_val = 0;
        int second_buy_qty_val = 0;
        double first_buy_ap = 0;
        double second_buy_ap = 0;
        double first_buy_percent = 0;
        double second_buy_percent = 0;
        double percent_diff = 0;
        string first_buy_order = "";
        string second_buy_order = "";

        string first_sell_sym = "";
        string second_sell_sym = "";
        int first_sell_qty_val = 0;
        int second_sell_qty_val = 0;
        double first_sell_cost_val = 0;
        double second_sell_cost_val = 0;
        double first_sell_ask_val = 0;
        double second_sell_ask_val = 0;
        double profit = 0;
        string first_sell_order = "";
        string second_sell_order = "";

        string stock_sym = "";
        double buy_diff_limit = 0;
        double sell_profit_limit = 0;
        Thread m_thread = null;
        Thread automation_thread = null;
        // Polygon Variables
        string polygon_api_key = "UFuTWoH6TgDvGfrqsZYSno10cb_BsRKc";
        // string polygon_api_key = "1cKoZ9IargrRy2pJiRnmoU8Txf93Cvt7";
        // string polygon_api_key = "XtR4bFOdgTdQLokZB7dNNXtwhTKa89Tv";
        string polygon_main_url = "https://api.polygon.io/v2/aggs/ticker/";
        WebSocket websocket = new WebSocket("wss://socket.polygon.io/stocks", sslProtocols: SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls);
        List<string> stocks = new List<string>();
        // Boolean websocket_flag = true;
        double first_close_price_buy = 0;
        double second_close_price_buy = 0;
        double first_open_price_buy = 0;
        double second_open_price_buy = 0;
        public MainWindow()
        {
            InitializeComponent();
            primary_default();
            socket_connection();
            //TimeZoneInfo Eastern_Standard_Time = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime dateTime_Eastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Eastern_Standard_Time);
            //if ( dateTime_Eastern.Month == 7 && (dateTime_Eastern.Day < 29 && dateTime_Eastern.Day > 24))
            //{
            //    InitializeComponent();
            //    primary_default();
            //    socket_connection();
            //} else
            //{
            //    System.Windows.MessageBox.Show("You got mistake.", "Warning!");
            //    this.Close();
            //}
        }

        private void socket_connection()
        {
            using (var reader = new StreamReader("./stocks.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    stocks.Add(values[0]);
                }
            }
            websocket.Opened += websocket_Opened;
            websocket.Error += websocket_Error;
            websocket.Closed += websocket_Closed;
            websocket.MessageReceived += websocket_MessageReceived;
            websocket.Open();
        }

        private void primary_default()
        {
            first_buy_symbol.Content = first_buy_sym;
            second_buy_symbol.Content = second_buy_sym;
            buy_diff.Text = buy_diff_limit.ToString();
            buy_diff_status.Content = "0";
            first_buy_qty.Text = first_buy_qty_val.ToString();
            second_buy_qty.Text = first_buy_qty_val.ToString();

            first_sell_symbol.Content = first_sell_sym;
            second_sell_symbol.Content = second_sell_sym;
            sell_diff.Text = sell_profit_limit.ToString();
            sell_diff_status.Content = "0";
            first_sell_qty.Text = first_sell_qty_val.ToString();
            second_sell_qty.Text = first_sell_qty_val.ToString();
            first_cost.Text = first_sell_cost_val.ToString();
            second_cost.Text = second_sell_cost_val.ToString();

            first_buy_delete.IsEnabled = false;
            second_buy_delete.IsEnabled = false;
            first_sell_delete.IsEnabled = false;
            second_sell_delete.IsEnabled = false;
            cancel_automation.IsEnabled = false;
        }

        // Automation + amount action
        private void amount_add_Click(object sender, RoutedEventArgs e)
        {
            if ( first_buy_sym == "" || second_buy_sym == "" )
            {
                System.Windows.MessageBox.Show("Please add stock symbol at first.", "ADD Amount - Warning!");
            } else
            {
                if ( first_buy_ap == 0 || second_buy_ap == 0 )
                {
                    System.Windows.MessageBox.Show("Buy ask price is not set from the polygon.", "Data connection error - Warning!");
                } else
                {
                    amount_value = Convert.ToDouble(amount.Text.Trim().ToString()) / 2;
                    if ( amount_value < first_buy_ap || amount_value < second_buy_ap ) 
                    {
                        System.Windows.MessageBox.Show("You set too small amount than price!.", "Small Amount - Warning!");
                    } else
                    {
                        first_buy_qty_val = Convert.ToInt32(amount_value / first_buy_ap);
                        first_buy_qty.Text = first_buy_qty_val.ToString();
                        second_buy_qty_val = Convert.ToInt32(amount_value / second_buy_ap);
                        second_buy_qty.Text = second_buy_qty_val.ToString();
                    }
                }
            }
        }

        private void Start_Automation_Click(object sender, RoutedEventArgs e)
        {
            if ( amount_value == 0 )
            {
                System.Windows.MessageBox.Show("Please add amount at first.", "Start Automation - Warning!");
            } 
            if ( first_buy_qty_val == 0 || second_buy_qty_val == 0 )
            {
                System.Windows.MessageBox.Show("Please add stock symbols check data connection to the Polygon at first.", "Start Automation - Warning!");
            }
            //else if ( buy_diff_limit == 0 )
            //{
            //    System.Windows.MessageBox.Show("Please add buy limits at first.", "Start Automation - Warning!");
            //}
            if (sell_profit_limit == 0)
            {
                System.Windows.MessageBox.Show("If not set sell amount, Will set as 0.01.", "Start Automation - Notificaton!");
                sell_profit_limit = 0.01;
                sell_diff.Text = "0.01";
            }
            automation_flag = true;
            automation_thread = new Thread(run_automation);
            automation_thread.Start();
            start_automation.IsEnabled = false;
            cancel_automation.IsEnabled = true;
        }

        private void Cancel_Automation_Click(object sender, RoutedEventArgs e)
        {
            if ( automation_flag && automation_thread != null )
            {
                automation_thread.Abort();
                cancel_automation.IsEnabled = false;
            }
        }

        private void run_automation()
        {
            while ( true )
            {
                buy_action();
                this.Dispatcher.Invoke(() =>
                {
                    buy_status.Content = "Waiting";
                });
                sell_action();
                this.Dispatcher.Invoke(() =>
                {
                    sell_status.Content = "Waiting";
                });
                System.Threading.Thread.Sleep(500);
            }
        }
        // buy part
        private void Buy_stock_changed(object sender, TextChangedEventArgs e)
        {
            stock_sym = buy_stock.Text.Trim();
        }
        private void First_add(object sender, RoutedEventArgs e)
        {
            if ( stocks.Contains(stock_sym) )
            {
                if (first_buy_sym == "")
                {
                    first_buy_sym = stock_sym;
                    get_close(first_buy_sym, true);
                    first_buy_symbol.Content = first_buy_sym + " (" + first_close_price_buy.ToString() + " : "+ first_open_price_buy.ToString() + " )";
                }
                else if (second_buy_sym == "")
                {
                    second_buy_sym = stock_sym;
                    get_close(second_buy_sym, false);
                    second_buy_symbol.Content = second_buy_sym + " (" + second_close_price_buy.ToString() + " : " + second_open_price_buy.ToString() + " )";
                    //if ( websocket_flag )
                    //{
                    //    websocket.Close();
                    //    websocket_flag = false;
                    //}
                    //websocket.Open();
                    //websocket_flag = true;
                }
                else
                {
                    first_buy_sym = stock_sym;
                    get_close(first_buy_sym, true);
                    first_buy_symbol.Content = first_buy_sym + " (" + first_close_price_buy.ToString() + " : " + first_open_price_buy.ToString() + " )";
                    second_buy_sym = "";
                    second_buy_symbol.Content = second_buy_sym;
                }
                stock_sym = "";
                buy_stock.Text = "";
            } else
            {
                System.Windows.MessageBox.Show("Please add stock symbol as correctly.", "ADD SYMBOL - Error");
                buy_stock.Text = "";
            }
        }
        private void Buy_save(object sender, RoutedEventArgs e)
        {
            if (first_buy_sym == "" || second_buy_sym == "")
            {
                System.Windows.MessageBox.Show("Please add buy stock symbol as correctly.", "ADD SYMBOL");
            }
            else if (buy_diff.Text == "")
            {
                System.Windows.MessageBox.Show("Please set difference amount as correctly.", "SET DIFF");
            }
            else if (first_buy_qty.Text == "" || second_buy_qty.Text == "")
            {
                System.Windows.MessageBox.Show("Please set qty value.", "SET QTY");
            }
            else
            {
                buy_status.Content = "Waiting...";
                buy_ui_action(false);
                if (first_buy_qty.Text.ToString().Trim() != "")
                {
                    first_buy_qty_val = int.Parse(first_buy_qty.Text);
                }
                else
                {
                    first_buy_qty_val = 0;
                }
                if (second_buy_qty.Text.ToString().Trim() != "")
                {
                    second_buy_qty_val = int.Parse(second_buy_qty.Text);
                }
                else
                {
                    second_buy_qty_val = 0;
                }
                buy_diff_limit = Convert.ToDouble(buy_diff.Text);
                m_thread = new Thread(buy_action);
                m_thread.Start();
            }
        }
        private void Buy_cancel(object sender, RoutedEventArgs e)
        {
            //first_buy_sym = "";
            //first_buy_symbol.Content = first_buy_sym;
            //second_buy_sym = "";
            //second_buy_symbol.Content = second_buy_sym;
            delete_order(first_buy_order);
            delete_order(second_buy_order);
            first_buy_delete.IsEnabled = false;
            second_buy_delete.IsEnabled = false;
            buy_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        private void buy_action()
        {
            while (true)
            {
                try
                {
                    //    string url = main_url + "marketdata/quotes";
                    //    string postdata = "?apikey=" + Uri.EscapeDataString(client_id);
                    //    postdata += "&symbol=" + Uri.EscapeDataString(first_buy_sym + "," + second_buy_sym);
                    //    JObject res = getRequest(url + postdata, false);
                    //    if (res != null)
                    //    {
                    //        percent_diff = first_buy_percent + second_buy_percent;
                    //        this.Dispatcher.Invoke(() =>
                    //        {
                    //            first_change.Content = first_buy_percent.ToString();
                    //            second_change.Content = second_buy_percent.ToString();
                    //            buy_diff_status.Content = percent_diff.ToString();
                    //        });
                    System.Threading.Thread.Sleep(5);
                    if (percent_diff <= buy_diff_limit)
                    {
                        //float first_buy_price = float.Parse(res[first_buy_sym]["askPrice"].ToString());
                        this.Dispatcher.Invoke(() =>
                        {
                            first_buy_qty_val = Convert.ToInt32(amount_value / first_buy_ap);
                            first_buy_qty.Text = first_buy_qty_val.ToString();
                        });
                        first_sell_cost_val = first_buy_ap;
                        first_buy_order = create_order(first_buy_qty_val, float.Parse(first_buy_ap.ToString()), first_buy_sym, "BUY", true);
                        this.Dispatcher.Invoke(() =>
                        {
                            if (first_buy_order != "FILLED")
                            {
                                first_buy_delete.IsEnabled = true;
                            }
                        });

                        this.Dispatcher.Invoke(() =>
                        {
                            second_buy_qty_val = Convert.ToInt32(amount_value / second_buy_ap);
                            second_buy_qty.Text = second_buy_qty_val.ToString();
                        });
                        second_sell_cost_val = second_buy_ap;
                        second_buy_order = create_order(second_buy_qty_val, float.Parse(second_buy_ap.ToString()), second_buy_sym, "BUY", true);
                        this.Dispatcher.Invoke(() =>
                        {
                            if (second_buy_order != "FILLED")
                            {
                                second_buy_delete.IsEnabled = true;
                            }
                            buy_status.Content = "Pending";
                        });
                        break;
                    }
                    //}
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, " Error! ");
                    System.Threading.Thread.Sleep(1000);
                }
            }

            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(3000);
                    if ( first_buy_order == "REJECTED" )
                    {
                        create_order(first_buy_qty_val, float.Parse(first_buy_ap.ToString()), first_buy_sym, "BUY", false);
                    } else
                    {
                        if (first_buy_order != "Filled")
                        {
                            if ( exist_order(first_buy_order) == true )
                            {
                                delete_order(first_buy_order);
                                create_order(first_buy_qty_val, float.Parse(first_buy_ap.ToString()), first_buy_sym, "BUY", false);
                            }
                        }
                    }
                    if ( second_buy_order == "REJECTED" )
                    {
                        create_order(second_buy_qty_val, float.Parse(second_buy_ap.ToString()), second_buy_sym, "BUY", false);
                    }
                    else
                    {
                        if (second_buy_order != "Filled")
                        {
                            if ( exist_order(second_buy_order) == true )
                            {
                                delete_order(second_buy_order);
                                create_order(second_buy_qty_val, float.Parse(second_buy_ap.ToString()), second_buy_sym, "BUY", false);
                            }
                        }
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        buy_status.Content = "Order Complete";
                        sell_panel.IsEnabled = true;
                        buy_stock_add_panel.IsEnabled = true;
                        first_buy_qty.IsEnabled = true;
                        second_buy_qty.IsEnabled = true;
                        buy_diff.IsEnabled = true;
                        buy_save.IsEnabled = true;

                        first_buy_delete.IsEnabled = false;
                        second_buy_delete.IsEnabled = false;
                        // Auto fill sell symbols
                        first_sell_sym = first_buy_sym;
                        second_sell_sym = second_buy_sym;
                        first_sell_qty_val = first_buy_qty_val;
                        second_sell_qty_val = second_buy_qty_val;

                        first_sell_symbol.Content = first_sell_sym;
                        second_sell_symbol.Content = second_sell_sym;
                        first_sell_qty.Text = first_sell_qty_val.ToString();
                        second_sell_qty.Text = second_sell_qty_val.ToString();
                        first_cost.Text = first_buy_ap.ToString();
                        second_cost.Text = second_buy_ap.ToString();
                    });
                    break;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, " Error! ");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        private void first_buy_delete_Click(object sender, RoutedEventArgs e)
        {
            first_buy_sym = "";
            first_buy_symbol.Content = first_buy_sym;
            delete_order(first_buy_order);
            first_buy_delete.IsEnabled = false;
            buy_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        private void second_buy_delete_Click(object sender, RoutedEventArgs e)
        {
            second_buy_sym = "";
            second_buy_symbol.Content = second_buy_sym;
            delete_order(second_buy_order);
            second_buy_delete.IsEnabled = false;
            buy_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        // sell part
        private void First_sell_qty_change(object sender, TextChangedEventArgs e)
        {
            if (first_sell_qty.Text != null && first_sell_qty.Text != "")
            {
                first_sell_qty_val = int.Parse(first_sell_qty.Text);
            }
            else
            {
                first_sell_qty_val = 0;
            }
        }
        private void Second_sell_qty_change(object sender, TextChangedEventArgs e)
        {
            if (second_sell_qty.Text != null && second_sell_qty.Text != "")
            {
                second_sell_qty_val = int.Parse(second_sell_qty.Text);
            }
            else
            {
                second_sell_qty_val = 0;
            }
        }
        private void Second_sell_cost_change(object sender, TextChangedEventArgs e)
        {
            if (second_cost.Text != null && second_cost.Text != "")
            {
                second_sell_cost_val = Convert.ToDouble(second_cost.Text);
            }
            else
            {
                second_sell_cost_val = 0;
            }
        }
        private void First_sell_cost_change(object sender, TextChangedEventArgs e)
        {
            if (first_cost.Text != null && first_cost.Text != "")
            {
                first_sell_cost_val = Convert.ToDouble(first_cost.Text);
            }
            else
            {
                first_sell_cost_val = 0;
            }
        }
        private void Sell_stock_changed(object sender, TextChangedEventArgs e)
        {
            stock_sym = sell_stock.Text.Trim();
        }
        private void Second_add(object sender, RoutedEventArgs e)
        {
            if ( stocks.Contains(stock_sym) )
            {
                if (first_sell_sym == "")
                {
                    first_sell_sym = stock_sym;
                    first_sell_symbol.Content = first_sell_sym;
                }
                else if (second_sell_sym == "")
                {
                    second_sell_sym = stock_sym;
                    second_sell_symbol.Content = second_sell_sym;
                    //if ( websocket_flag )
                    //{
                    //    websocket.Close();
                    //    websocket_flag = false;
                    //}
                    //websocket.Open();
                    //websocket_flag = true;
                }
                else
                {
                    first_sell_sym = stock_sym;
                    first_sell_symbol.Content = first_sell_sym;
                    second_sell_sym = "";
                    second_sell_symbol.Content = second_sell_sym;
                    //websocket.Close();
                    //websocket_flag = false;
                }
                stock_sym = "";
                sell_stock.Text = "";
            } else
            {
                System.Windows.MessageBox.Show("Please add stock symbol as correctly.", "ADD SYMBOL - Error");
                sell_stock.Text = "";
            }
        }
        private void Sell_save(object sender, RoutedEventArgs e)
        {
            if (first_sell_sym == "" || second_sell_sym == "")
            {
                System.Windows.MessageBox.Show("Please add sell stock symbol as correctly.", "ADD SYMBOL");
            }
            else if (sell_diff.Text == "")
            {
                System.Windows.MessageBox.Show("Please set profit/loss amount as correctly.", "SET PROFIT/LOSS");
            }
            else if (first_sell_qty.Text == "" || second_sell_qty.Text == "")
            {
                System.Windows.MessageBox.Show("Please set qty value as correctly.", "SET QTY");
            }
            else if (first_cost.Text == "" || second_cost.Text == "")
            {
                System.Windows.MessageBox.Show("Please set cost to sell.", "SET SELL COST");
            }
            else
            {
                sell_ui_action(false);
                sell_profit_limit = Convert.ToDouble(sell_diff.Text);
                m_thread = new Thread(sell_action);
                m_thread.Start();
            }
        }
        private void Sell_cancel(object sender, RoutedEventArgs e)
        {
            //first_sell_sym = "";
            //first_sell_symbol.Content = first_sell_sym;
            //second_sell_sym = "";
            //second_sell_symbol.Content = second_sell_sym;
            delete_order(first_sell_order);
            delete_order(second_sell_order);
            first_sell_delete.IsEnabled = false;
            second_sell_delete.IsEnabled = false;
            sell_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        private void first_sell_delete_Click(object sender, RoutedEventArgs e)
        {
            first_sell_sym = "";
            first_sell_symbol.Content = first_sell_sym;
            delete_order(first_sell_order);
            first_sell_delete.IsEnabled = false;
            sell_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }

        private void second_sell_delete_Click(object sender, RoutedEventArgs e)
        {
            second_sell_sym = "";
            second_sell_symbol.Content = second_sell_sym;
            delete_order(second_sell_order);
            second_sell_delete.IsEnabled = false;
            sell_ui_action(true);
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        private void sell_action()
        {
            while (true)
            {
                try 
                {
                    System.Threading.Thread.Sleep(5);
                    //string url = main_url + "marketdata/quotes";
                    //string postdata = "?apikey=" + Uri.EscapeDataString(client_id);
                    //postdata += "&symbol=" + Uri.EscapeDataString(first_sell_sym + "," + second_sell_sym);
                    //JObject res = getRequest(url + postdata, false);
                    //    first_sell_ask_val = Convert.ToDouble(res[first_sell_sym]["bidPrice"].ToString());
                    //    second_sell_ask_val = Convert.ToDouble(res[second_sell_sym]["bidPrice"].ToString());
                    //    double first_profit_val = 0;
                    //    double second_profit_val = 0;
                    //    if (first_sell_qty_val > 0 && first_sell_cost_val > 0)
                    //    {
                    //        first_profit_val = (first_sell_cost_val - first_sell_ask_val) * first_sell_qty_val;
                    //    }
                    //    if (second_sell_qty_val > 0 && second_sell_cost_val > 0)
                    //    {
                    //        second_profit_val = (second_sell_cost_val - second_sell_ask_val) * second_sell_qty_val;
                    //    }
                    //    profit = first_profit_val + second_profit_val;
                    //    this.Dispatcher.Invoke(() =>
                    //    {
                    //        second_sell_price.Content = second_sell_ask_val.ToString();
                    //        first_sell_price.Content = first_sell_ask_val.ToString();
                    //        sell_diff_status.Content = String.Format("{0:0.00}", profit);
                    //    });
                    if (profit >= sell_profit_limit)
                    {
                        first_sell_order = create_order(first_sell_qty_val, float.Parse(first_sell_ask_val.ToString()), first_sell_sym, "SELL", true);
                        this.Dispatcher.Invoke(() =>
                        {
                            if (first_sell_order != "FILLED")
                            {
                                first_sell_delete.IsEnabled = true;
                            }
                        });
                        second_sell_order = create_order(second_sell_qty_val, float.Parse(second_sell_ask_val.ToString()), second_sell_sym, "SELL", true);
                        this.Dispatcher.Invoke(() =>
                        {
                            if (second_sell_order != "FILLED")
                            {
                                second_sell_delete.IsEnabled = true;
                            }
                            sell_status.Content = "Pending";
                        });
                        break;
                    }
                    //}
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, " Error! ");
                    System.Threading.Thread.Sleep(1000);
                }
            }

            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(3000);
                    if (first_sell_order == "REJECTED")
                    {
                        create_order(first_sell_qty_val, float.Parse(first_sell_ask_val.ToString()), first_sell_sym, "SELL", false);
                    }
                    else
                    {
                        if (first_sell_order != "Filled")
                        {
                            if (exist_order(first_sell_order) == true)
                            {
                                delete_order(first_sell_order);
                                create_order(first_sell_qty_val, float.Parse(first_sell_ask_val.ToString()), first_sell_sym, "SELL", false);
                            }
                        }
                    }
                    if (second_sell_order == "REJECTED")
                    {
                        create_order(second_sell_qty_val, float.Parse(second_sell_ask_val.ToString()), second_sell_sym, "SELL", false);
                    }
                    else
                    {
                        if (second_sell_order != "Filled")
                        {
                            if (exist_order(second_sell_order) == true)
                            {
                                delete_order(second_sell_order);
                                create_order(second_sell_qty_val, float.Parse(second_sell_ask_val.ToString()), second_sell_sym, "SELL", false);
                            }
                        }
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        sell_status.Content = "Order Complete";
                        buy_panel.IsEnabled = true;
                        sell_stock_add_panel.IsEnabled = true;
                        first_sell_qty.IsEnabled = true;
                        second_sell_qty.IsEnabled = true;
                        first_cost.IsEnabled = true;
                        second_cost.IsEnabled = true;
                        sell_diff.IsEnabled = true;
                        sell_save.IsEnabled = true;
                        first_sell_delete.IsEnabled = false;
                        second_sell_delete.IsEnabled = false;
                    });
                    break;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, " Error! ");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        // api using functions 
        private Boolean exist_order(string order_id)
        {
            if (order_id == "FILLED")
            {
                return false;
            }
            else
            {
                try
                {
                    string url = main_url + "accounts/" + account_id + "/orders/" + order_id;
                    JObject res = getRequest(url, true);
                    Console.WriteLine("check exist order ! : " + res["status"] + " : status ");
                    if (res == null)
                    {
                        return false;
                    }
                    else if (res["status"].ToString() == "WORKING" || res["status"].ToString() == "PENDING" )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, " Error! ");
                    return false;
                }
            }
        }
        private string create_order(int qty, float price, string sym, string flag, Boolean order_type_flag)
        {
            string url = main_url + "accounts/" + account_id + "/orders";
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["Authorization"] = "Bearer " + get_access_token();
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            var data = "";
            if ( order_type_flag )
            {
                data = @"{
                    ""session"": ""NORMAL"",
                    ""duration"": ""DAY"",
                    ""price"": 9.0,
                    ""orderType"": ""LIMIT"",
                    ""orderStrategyType"": ""SINGLE"",
                    ""orderLegCollection"": [
                        {
                            ""instrument"": {
                                ""assetType"": ""EQUITY"",
                                ""symbol"": ""AMZN""
                            },
                            ""instruction"": ""Buy"",
                            ""quantity"": 1,
                        }
                    ]
                }";                
            } else
            {
                data = @"{
                    ""session"": ""NORMAL"",
                    ""duration"": ""DAY"",
                    ""orderType"": ""MARKET"",
                    ""orderStrategyType"": ""SINGLE"",
                    ""orderLegCollection"": [
                        {
                            ""instrument"": {
                                ""assetType"": ""EQUITY"",
                                ""symbol"": ""AMZN""
                            },
                            ""instruction"": ""Buy"",
                            ""quantity"": 1,
                            }
                        ]
                }";
            }            
            JObject order_data = JObject.Parse(data);
            if ( order_type_flag )
            {
                order_data["price"] = price;
            }
            order_data["orderLegCollection"][0]["instrument"]["symbol"] = sym;
            order_data["orderLegCollection"][0]["quantity"] = qty;
            order_data["orderLegCollection"][0]["instruction"] = flag.ToUpper();
            data = Convert.ToString(order_data);
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }
            WebResponse webResponse = httpRequest.GetResponse();
            Stream webStream = webResponse.GetResponseStream();
            StreamReader responseReader = new StreamReader(webStream);
            string result = responseReader.ReadToEnd();
            responseReader.Close();
            // get orders of this account
            TimeZoneInfo Eastern_Standard_Time = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime dateTime_Eastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Eastern_Standard_Time);
            url = main_url + "orders?accountId=" + account_id + "&maxResults=200&fromEnteredTime=" + dateTime_Eastern.ToString("yyyy-MM-dd");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "Bearer " + get_access_token();
            System.Threading.Thread.Sleep(50);
            WebResponse webResponse_orders = request.GetResponse();
            Stream webStream_orders = webResponse_orders.GetResponseStream();
            StreamReader responseReader_orders = new StreamReader(webStream_orders);
            string result_orders = responseReader_orders.ReadToEnd();
            if (result_orders.Contains("\n"))
            {
                result_orders = result_orders.Replace("\n", "");
            }
            if (result_orders == "" )
            {
                return "FILLED";

            } 
            else
            {
                JObject[] orders = JsonConvert.DeserializeObject<JObject[]>(result_orders);
                string order_id = "";
                string order_status = "";
                foreach (JObject each_order in orders)
                {
                    if (int.Parse(each_order["quantity"].ToString()) == qty && each_order["orderLegCollection"][0]["instruction"].ToString() == flag.ToUpper() && each_order["orderLegCollection"][0]["instrument"]["symbol"].ToString() == sym)
                    {
                        try
                        {
                            var Timestamp_now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                            var Timestamp_entered = new DateTimeOffset(Convert.ToDateTime(each_order["enteredTime"])).ToUnixTimeSeconds();
                            if ( Timestamp_now < Timestamp_entered + 60 )
                            {
                                order_id = each_order["orderId"].ToString();
                                order_status = each_order["status"].ToString();
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                if ( order_status == "REJECTED")
                {
                    return order_status;
                }
                else if (order_status == "" || order_status == "FILLED" || order_status == "CANCELED")
                {
                    return "FILLED";
                }
                else
                {
                    if (order_status == "WORKING")
                    {
                        return order_id;
                    }
                    else
                    {
                        return "FILLED";
                    }
                }
            }
        }
        private Boolean delete_order(string order_id)
        {
            Console.WriteLine("delete_order ! : " + order_id + " : order_id ");
            if (order_id == "FILLED" || order_id == "error" || order_id == "")
            {
                return true;
            }
            try
            {
                string url = main_url + "accounts/" + account_id + "/orders/" + order_id;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "DELETE";
                request.Accept = "*/*";
                request.Headers["Authorization"] = "Bearer " + get_access_token();
                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string result = responseReader.ReadToEnd();
                return true;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, " Error! ");
                return false;
            }            
        }
        private string get_access_token()
        {
            string url = main_url + "oauth2/token";
            string postdata = "grant_type=" + Uri.EscapeDataString("refresh_token");
            postdata += "&refresh_token=" + Uri.EscapeDataString(refresh_token);
            postdata += "&client_id=" + Uri.EscapeDataString(client_id);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(postdata);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            WebResponse webResponse = request.GetResponse();
            Stream webStream = webResponse.GetResponseStream();
            StreamReader responseReader = new StreamReader(webStream);
            JObject response = JObject.Parse(responseReader.ReadToEnd());
            responseReader.Close();
            string access_token = response["access_token"].ToString();
            return access_token;
        }
        // Basic functions 
        public JObject getRequest(string url, Boolean auth_api)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "*/*";
                if (auth_api)
                {
                    request.Headers["Authorization"] = "Bearer " + get_access_token();
                }
                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string result = responseReader.ReadToEnd();
                if (result.Contains("\n"))
                {
                    result = result.Replace("\n", "");
                }
                JObject response = JObject.Parse(result);
                responseReader.Close();
                return response;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, " Error! ");
                return null;
            }
        }
        // UI manage functions
        private void buy_ui_action(Boolean status)
        {
            sell_panel.IsEnabled = status;
            buy_stock_add_panel.IsEnabled = status;
            first_buy_qty.IsEnabled = status;
            second_buy_qty.IsEnabled = status;
            buy_diff.IsEnabled = status;
            buy_save.IsEnabled = status;
        }
        private void sell_ui_action(Boolean status)
        {
            buy_panel.IsEnabled = status;
            sell_stock_add_panel.IsEnabled = status;
            first_sell_qty.IsEnabled = status;
            second_sell_qty.IsEnabled = status;
            first_cost.IsEnabled = status;
            second_cost.IsEnabled = status;
            sell_diff.IsEnabled = status;
            sell_save.IsEnabled = status;
        }

        // Websocket functions - polygon close of daily
        private void get_close(string sym, Boolean first_or_second)
        {
            string url = polygon_main_url + sym + "/prev" + "?adjusted=true&apiKey=" + polygon_api_key;
            Console.WriteLine("get_close! : " + url);
            JObject res = getRequest(url, false);
            if ( res != null )
            {
                if (first_or_second)
                {
                    first_close_price_buy = Convert.ToDouble(res["results"][0]["c"]);
                    first_open_price_buy = Convert.ToDouble(res["results"][0]["o"]);
                }
                else
                {
                    second_close_price_buy = Convert.ToDouble(res["results"][0]["c"]);
                    second_open_price_buy = Convert.ToDouble(res["results"][0]["o"]);
                }
            } else
            {
                System.Windows.MessageBox.Show(" Cant get close price from Polygon.io.", " Error! ");
                this.Close();
            }
            Console.WriteLine("get_close! : " + first_close_price_buy.ToString() + " : " + first_open_price_buy.ToString());
            Console.WriteLine("get_close! : " + second_close_price_buy.ToString() + " : " + second_open_price_buy.ToString());
        }
        private void websocket_Opened(object sender, EventArgs e)
        {
            //if (first_sell_sym != "" && second_sell_sym != "" && first_buy_sym != "" && second_buy_sym != "" )
            //{
            //    query = "Q." + first_sell_sym + ",Q." + second_sell_sym + ",Q." + first_buy_sym + ",Q." + second_buy_sym;
            //} else if (first_sell_sym != "" && second_sell_sym != "" )
            //{
            //    query = "Q." + first_sell_sym + ",Q." + second_sell_sym;
            //} else if (first_buy_sym != "" && second_buy_sym != "")
            //{
            //    query = ",Q." + first_buy_sym + '"' + ",Q." + second_buy_sym;
            //}
            if ( stocks.Count > 0 )
            {
                String query = "";
                for ( int i = 0; i < stocks.Count; i++)
                {
                    query = query + "Q." + stocks[i] + ",";
                }
                query = query.Remove(query.Length - 1);
                Console.WriteLine("Connected! : " + query);
                websocket.Send("{\"action\":\"auth\",\"params\":" + '"' + polygon_api_key + '"' + "}");
                websocket.Send("{\"action\":\"subscribe\",\"params\":" + '"' + query + '"' + "}");
            }
        }
        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("WebSocket Error");
            Console.WriteLine(e.Exception.Message);
        }
        private void websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Connection Closed...");
            // Add Reconnect logic... this.Start()
        }
        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Console.WriteLine(e.Message + "-----" + e.Message.GetType());
            String msg = e.Message;
            if ( msg.Contains("Connected Successfully") )
            {
                return;
            } else if (msg.Contains("authenticated")) {
                return;
            } else if (msg.Contains("subscribed to: Q."))
            {
                return;
            }
            try
            {
                List<StockSecond> items = JsonConvert.DeserializeObject<List<StockSecond>>(e.Message);
                //Console.WriteLine(e.Message + "-----");
                percent_diff = 0;
                profit = 0;
                Boolean f_b = false;
                Boolean s_b = false;
                Boolean f_s = false;
                Boolean s_s = false;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].sym == first_buy_sym)
                    {
                        // Console.WriteLine(items[i].ap.ToString() + "----- first buy sym");
                        first_buy_ap = Convert.ToDouble(items[i].ap.ToString());
                        f_b = true;
                    }
                    if (items[i].sym == second_buy_sym)
                    {
                        // Console.WriteLine(items[i].ap.ToString() + "----- second buy sym");
                        second_buy_ap = Convert.ToDouble(items[i].ap.ToString());
                        s_b = true;
                    }
                    if (items[i].sym == first_sell_sym)
                    {
                        // Console.WriteLine(items[i].bp.ToString() + "----- first sell sym");
                        first_sell_ask_val = Convert.ToDouble(items[i].bp.ToString());
                        f_s = true;
                    }
                    if (items[i].sym == second_sell_sym)
                    {
                        // Console.WriteLine(items[i].bp.ToString() + "----- second sell sym");
                        second_sell_ask_val = Convert.ToDouble(items[i].bp.ToString());
                        s_s = true;
                    }
                    if ( f_b && s_b && f_s && s_s )
                    {
                        break;
                    }
                }

                if (first_sell_sym != "" && second_sell_sym != "" )
                {
                    double first_profit_val = 0;
                    double second_profit_val = 0;
                    if (first_sell_qty_val > 0 && first_sell_cost_val > 0)
                    {
                        first_profit_val = ( first_sell_ask_val - first_sell_cost_val ) * first_sell_qty_val;
                    }
                    if (second_sell_qty_val > 0 && second_sell_cost_val > 0)
                    {
                        second_profit_val = ( second_sell_ask_val - second_sell_cost_val ) * second_sell_qty_val;
                    }
                    profit = Math.Round(first_profit_val + second_profit_val, 4);
                }

                if (first_buy_sym != "" && second_buy_sym != "")
                {
                    //first_buy_percent = Math.Round(Convert.ToDouble((first_close_price_buy - first_buy_ap) / first_buy_ap * 100), 4) * -1;
                    //second_buy_percent = Math.Round(Convert.ToDouble((second_close_price_buy - second_buy_ap) / second_buy_ap * 100), 4) * -1;
                    first_buy_percent = Math.Round(Convert.ToDouble((first_buy_ap - first_close_price_buy) / first_close_price_buy * 100), 4);
                    second_buy_percent = Math.Round(Convert.ToDouble((second_buy_ap - second_close_price_buy) / second_close_price_buy * 100), 4);
                    percent_diff = Math.Round(first_buy_percent + second_buy_percent, 4);
                }

                this.Dispatcher.Invoke(() =>
                {
                    first_change.Content = first_buy_percent.ToString();
                    second_change.Content = second_buy_percent.ToString();
                    buy_diff_status.Content = percent_diff.ToString();

                    second_sell_price.Content = second_sell_ask_val.ToString();
                    first_sell_price.Content = first_sell_ask_val.ToString();
                    sell_diff_status.Content = String.Format("{0:0.000}", profit);
                });
            }
            catch (Exception error)
            {
                System.Windows.MessageBox.Show(error.Message, " Error! ");
            }
        
        }

    }
}
