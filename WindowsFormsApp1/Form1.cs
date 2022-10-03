using Microsoft.Win32;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region Variables for TIA Portal projects
        /// <summary>
        /// variables to connect to Tia Portal
        /// </summary>
        private static TiaPortalProcess _tiaProcess;

        public TiaPortal MyTiaPortal
        {
            get; set;
        }

        public Project MyProject
        {
            get; set;
        }

        public PlcSoftware globalPlcSoftware;
       
        public Form1()
        {
            InitializeComponent();
            AppDomain CurrentDomain = AppDomain.CurrentDomain;
            CurrentDomain.AssemblyResolve += new ResolveEventHandler(MyResolver);
        }
        #endregion
        #region Assembly
        /// <summary>
        /// I need to know what is this part for
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly MyResolver(object sender, ResolveEventArgs args)
        {
            int index = args.Name.IndexOf(',');
            if (index == -1)
            {
                return null;
            }
            string name = args.Name.Substring(0, index);

            RegistryKey filePathReg = Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\\Siemens\\Automation\\Openness\\15.1\\PublicAPI\\15.1.0.0");

            if (filePathReg == null)
                return null;

            object oRegKeyValue = filePathReg.GetValue(name);
            if (oRegKeyValue != null)
            {
                string filePath = oRegKeyValue.ToString();

                string path = filePath;
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return Assembly.LoadFrom(fullPath);
                }
            }

            return null;
        }
        #endregion
        #region Connection to the TIA Project
        private void ConnectingTiaProject()
        {
            IList<TiaPortalProcess> processes = TiaPortal.GetProcesses();
            switch (processes.Count)
            {
                case 1:
                    _tiaProcess = processes[0];
                    MyTiaPortal = _tiaProcess.Attach();
                    
                    if (MyTiaPortal.Projects.Count <= 0)
                    {
                        txt_Status.Text = "No TIA Portal Project was found!";
                        return;
                    }
                    MyProject = MyTiaPortal.Projects[0];
                    break;
                case 0:
                    txt_Status.Text = "No running instance of TIA Portal was found!";
                    return;
                default:
                    txt_Status.Text = "More than one running instance of TIA Portal was found!";
                    return;
            }
            txt_Status.Text = _tiaProcess.ProjectPath.ToString();
        }
        #endregion
        #region Enumerate PLC Blocks
        private static void EnumeratePlcBlocks(PlcSoftware plcsoftware)
        //Enumerates all blocks
        {
            foreach (PlcBlock block in plcsoftware.BlockGroup.Blocks)
            {
                string message = $"{plcsoftware.Name}||{block.Name}({block.GetType().Name}{block.Number})";
                MessageBox.Show(message);
            }
        }
        #endregion
        #region GetPlcSoftware
        private PlcSoftware GetPlcSoftware(Device device)
        {
            DeviceItemComposition deviceItemComposition = device.DeviceItems;
            foreach (DeviceItem deviceItem in deviceItemComposition)
            {
                SoftwareContainer softwareContainer = deviceItem.GetService<SoftwareContainer>();
                if (softwareContainer != null)
                {
                    Software softwareBase = softwareContainer.Software;
                    PlcSoftware plcSoftware = softwareBase as PlcSoftware;
                    return plcSoftware;
                }
            }
            return null;
        }
      
        #region Get List of PLC Blocks
        private void GetListOfPlcBlocks()
        {
            DeviceComposition deviceComposition = MyProject.Devices;
            foreach (Device device in deviceComposition)
            {
                PlcSoftware plcSoftware = GetPlcSoftware(device);
                EnumeratePlcBlocks(plcSoftware);
            }
            
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            ConnectingTiaProject();
            GetListOfPlcBlocks();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
