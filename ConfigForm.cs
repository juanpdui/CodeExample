using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZSDK_API.ApiException;
using ZSDK_API.Comm;
using ZSDK_API.Printer;
using ZSDK_API.Discovery;
using System.Xml;
using System.IO;

namespace ReplicasV220
{

    //Clase y formulario para la configuración del SW REPLICAS V220
    public partial class ConfigForm : Form
    {

        private String macAddress = "";
        private ZebraPrinterConnection connection;
        private ZebraPrinter printer;
        private String password;
        

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            disconnect();
            this.Hide();
            MainForm frm = new MainForm();
            frm.Show();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //System.Console.WriteLine("Entro en metodo de botonsito");
            //System.Diagnostics.Debug.WriteLine("your message here");
            System.Diagnostics.Debug.WriteLine("entro al metodo del boton");
            if (connection != null && connection.IsConnected())
            {
                connection.Close();

            }

            MsjLbl.Text = "Buscando...";
            //System.Console.WriteLine("inicia proceso de discovery");
            System.Diagnostics.Debug.WriteLine("Inicia el proeso de discovery");


            PrinterBox.Items.Clear();
            Thread t = new Thread(doBluetoothDiscovery);
            t.Start();

           
        }

        private void doBluetoothDiscovery()
        {
            //System.Console.WriteLine("Entro en metodo de doBluetoothDiscovery");
            System.Diagnostics.Debug.WriteLine("Entro en metodo de doBluetoothDiscovery");
            String cadena = "";
            Thread.Sleep(1000);
            try
            {
                DiscoveredPrinter[] printers = BluetoothDiscoverer.FindPrinters();
                //PrinterBox.BeginUpdate();
                for (int i = 0; i <= printers.Length-1; i++)
                {
                    System.Console.WriteLine("Entro en el for lazo:" + i);
                    System.Diagnostics.Debug.WriteLine("entro en el lazo for");
                    //PrinterBox.Items.Add(""+ i + ". " + printers[i].ToString);
                    //PrinterBox.Items.Add(printers[i].ToString);

                    cadena = printers[i].ToString();
                    
                    System.Diagnostics.Debug.WriteLine(i+ ". "+cadena);
                    //PrinterBox.Items.Add(cadena);
                    //El control debe ser cambiado por el hilo que lo creo, es decir por el hilo de la misma GUI
                    //Se crea un invocador, mediante un metodo invoke se llama al thread del GUI y se puede controlar el objeto del usuario
                    setComboBox(cadena);


                    

                }
                setLbl("Listo!, Deslizar Lista");
                //MsjLbl.Text = "Impresoras Enlistadas";
                //PrinterBox.EndUpdate();


            }
            catch (DiscoveryException ex)
            {
                DialogResult Msj;

                Msj = MessageBox.Show("Error al tratar de obtener los dispositivos");
                System.Console.WriteLine("Error: " + ex.Message);

            }
        }

        delegate void SetComboBoxCallBack(string text);

        private void setComboBox(string text)
        {
            if (this.PrinterBox.InvokeRequired)
            {
                SetComboBoxCallBack d = new SetComboBoxCallBack(setComboBox);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.PrinterBox.Items.Add(text);
                //macAddress = this.PrinterBox.SelectedItem.ToString();

            }
        }

        private void displayPrinters(DiscoveredPrinter[] printers)
        {
            MsjLbl.Text = "Dispositivos encontrados: " + printers.Length;
            //BeginInvoke(new MyProgressEventsHandler(Upd
            for (int i=0;i<printers.Length;i++){
//                PrinterBox.Items.Add ="";
            }

        }




        public void updateButtonFromWorkerThread(bool enabled)
        {
            Invoke(new ButtonStatusHandler(UpdateButton), enabled);
            
        }

        private delegate void ButtonStatusHandler(bool enabled);
        private delegate void MyProgressEventsHandler(object sender, DiscoveredPrinter[] printers);

        private void UpdateButton(bool enabled)
        {
            
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                XmlReader r = XmlReader.Create("AppConfig.xml");
                r.ReadStartElement("ConfigReplicasV220");
                ServerTxt.Text = r.ReadElementContentAsString();
                PCopyTxt.Text = r.ReadElementContentAsString();
//                PrinterBox.Items.Add(r.ReadElementContentAsString());
                //PrinterBox.Show(r.ReadElementContentAsString().ToString);
                //PrinterBox.SelectedValue = (String)r.ReadElementContentAsString();
                PrinterBox.Items.Add(r.ReadElementContentAsString());
                PrinterBox.SelectedIndex=0;
                password = r.ReadElementContentAsString();
                r.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se encuentra el archivo de configuración, reinstale la aplicación o comuníquese con Uniscan");
                System.Diagnostics.Debug.WriteLine("Error al leer XML: " + ex.StackTrace.ToString());

            }



             /*w.WriteStartElement("ConfigReplicasV220");
                    w.WriteElementString("ServerTxt", ServerTxt.Text);
                    w.WriteElementString("PCopyTxt", PCopyTxt.Text);
                    w.WriteElementString("PrinterBox", PrinterBox.SelectedItem.ToString());
                    w.WriteElementString("PassTxt", PassTxt.Text);
                    w.Close();*/
        }

        private void PrinterBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Guarda la mac seleccionada y lo envia el hilo doConnectBT para realizar el emparejamiento
            macAddress = (String)PrinterBox.SelectedItem;
            Thread t = new Thread(doConnectBT);
            t.Start();

        }

        private void doConnectBT()
        {

            if (connection != null)
            {
                doDisconnect();
            }

            if (this.macAddress == null || this.macAddress.Length != 12)
            {
                //MsjLbl.Text = "Dirección MAC Inválida";
                setLbl("Dirección MAC Inválida");
                disconnect();

            }
            else
            {
                //MsjLbl.Text = "Conectando... espere";
                setLbl("Conectando... espere");
                try
                {
                    connection = new BluetoothPrinterConnection(this.macAddress);
                    connection.Open();
                    Thread.Sleep(1000);
                }
                catch (ZebraPrinterConnectionException)
                {
                    
                    MessageBox.Show("Deshabilitado para realizar conexiones a la impresora");
                    disconnect();
                    
                }
                catch (ZebraException)
                {
                    
                    MessageBox.Show("Error de comunicación con la impresora");
                    disconnect();
                }
                
                printer = null;
                if (connection != null && connection.IsConnected())
                {
                 //MsjLbl.Text = "Obteniendo impresora";
                 setLbl("Obteniendo impresora");
                 Thread.Sleep(1000);

                 printer = ZebraPrinterFactory.GetInstance(connection);
                 //MsjLbl.Text = "Obteniendo LP";
                 setLbl("Obteniendo LP");
                 Thread.Sleep(1000);

                 PrinterLanguage pl = printer.GetPrinterControlLanguage();
                 //MsjLbl.Text = "LP: "+pl.ToString();
                 setLbl("LP: "+pl.ToString());
                 Thread.Sleep(1000);

                                
                 //MsjLbl.Text = "Conectado a: "+macAddress;
                 setLbl("Conectado a: "+macAddress);

                 //Etiqueta de configuración
                    //printer.GetToolsUtil().PrintConfigurationLabel();
                 Thread.Sleep(1000);
                   //printer.GetToolsUtil().SendCommand(ZPLString);

                    //antes de enviar comandos ZPL primero se debe calibrar la impresora, enviar comando de calíbración
                 setLbl("Calibrando...");
                 printer.GetToolsUtil().Calibrate();
                 setLbl("Conectado a: " + macAddress);
                 
                 //printer.GetFileUtil().SendFileContents("D:\test4.prn");
                   connection.Close();


                }

                }
        }

        delegate void SetLblCallBack(string text);

        public void setLbl(string text)
        {
            if (this.MsjLbl.InvokeRequired)
            {
                SetLblCallBack d = new SetLblCallBack(setLbl);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.MsjLbl.Text = text;
            }
        }


        public void disconnect()
        {
            Thread t = new Thread(doDisconnect);
            t.Start();

        }


        private void doDisconnect()
        {
            try
            {
                if (connection != null && connection.IsConnected())
                {
                    connection.Close();
                }
            }
            catch (ZebraException)
            {
                
                MessageBox.Show("Error, puerto COM desconectado!");
            }
            Thread.Sleep(1000);
            MessageBox.Show("No conectado");
            connection = null;

        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            

            if (ServerTxt.Text.Length == 0 || PCopyTxt.Text.Length == 0 || (String)PrinterBox.SelectedValue == "")
                {
                    MessageBox.Show("Todos los campos deben estar llenados adecuadamente");
                    ServerTxt.Focus();
                }

            else if ((PassTxt.Text.Equals(VerPassTxt.Text.ToString()) == true))
            {
                System.Diagnostics.Debug.WriteLine("Información del PrinterBox: " + PrinterBox.SelectedItem.ToString());

                try
                {


                    if (File.Exists("AppConfig.xml"))
                    {
                        File.Delete("AppConfig.xml");
                    }

                    XmlWriter w = XmlWriter.Create("AppConfig.xml");
                                
                    System.Diagnostics.Debug.WriteLine("Se ha creado el archivo AppConfig.Xml: ");

                    w.WriteStartElement("ConfigReplicasV220");
                    w.WriteElementString("ServerTxt", ServerTxt.Text);
                    w.WriteElementString("PCopyTxt", PCopyTxt.Text);
                    w.WriteElementString("PrinterBox", PrinterBox.SelectedItem.ToString());
                    if (PassTxt.Text.Length == 0 && VerPassTxt.Text.Length == 0)
                    {
                        w.WriteElementString("PassTxt", password);
                    }
                    else
                    {
                        w.WriteElementString("PassTxt", PassTxt.Text);
                    }
                    w.Close();

                    MessageBox.Show("La configuración ha sido guardada correctamente");
                    connection.Close();

                    this.Hide();
                    MainForm frm = new MainForm();
                    frm.Show();
                    
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("Ocurrió un problema al generar el archivo de configuración. Reinstale el programa o comuníquese con Uniscan");
                    System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace.ToString());
                   
                    
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("Ha ocurrido un problema, favor comunicarse con el administrador o con Uniscan");
                    System.Diagnostics.Debug.WriteLine(ex2.StackTrace);
                }
            }
            else
            {
                MessageBox.Show("Las contraseñas no coinciden");
            }


            
        }

        private void label4_ParentChanged(object sender, EventArgs e)
        {

        }

        private void PCopyTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsSeparator(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

        }


    }
}
