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
using System.IO;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for BackupForm.xaml
    /// </summary>
    public partial class BackupForm : Window
    {
        public BackupForm()
        {
            InitializeComponent();
        }

        private void Backup_Window_Loaded(object sender, RoutedEventArgs e)
        {
            //populate the backup drive list box
            if (DriveInfo.GetDrives() != null)
            {
                string driveDesc;
                string driveVolLable;

                var driveList = DriveInfo.GetDrives();
                foreach (var drive in driveList)
                {
                    try
                    {
                        if (drive.VolumeLabel != "")
                        {
                            driveVolLable = drive.VolumeLabel;
                        } else
                        {
                            driveVolLable = "";
                        }
                        driveDesc = drive.Name;
                        driveDesc = driveDesc + " [" + driveVolLable + "]";
                        bkListBoxDrives.Items.Add(driveDesc);
                    }
                    catch
                    {

                    }

                }

            }
        }

        private void bkListBoxDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(bkListBoxDrives.SelectedItem != null)
            {
                bkButtonBackup.IsEnabled = true;
            } else
            {
                bkButtonBackup.IsEnabled = false;
            }

        }

        private void bkButtonBackup_Click(object sender, RoutedEventArgs e)
        {
            if (bkListBoxDrives.SelectedItem != null)
            {
                //make the filename
                DateTime currentDateTime = DateTime.Now;
                string bkfoldername = currentDateTime.ToString("ddMMyyy");
                bkfoldername = bkfoldername + "-";
                bkfoldername = bkfoldername + currentDateTime.ToString("HHmmss");

                string backupDrive = bkListBoxDrives.SelectedItem.ToString().Substring(0, 2);
                backupDrive = backupDrive + "/LaundryBackup";

                string fullBackupFolderPath;
                fullBackupFolderPath = backupDrive + "/" + bkfoldername;

                //string backupFolder = backupDrive + "/" + 

                //check to see if the backup folder exists
                if (Directory.Exists(backupDrive))
                {
                    //check for the dated backup folder
                    //backupDrive = backupDrive + "/" + bkfoldername;
                    Directory.CreateDirectory(fullBackupFolderPath);
                } else //create the backup folder
                {
                    Directory.CreateDirectory(backupDrive);
                    Directory.CreateDirectory(fullBackupFolderPath);
                }

                string workingFilesPath;
                workingFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Laundry/";

                //create the full directory structure
                string[] dirList = Directory.GetDirectories(workingFilesPath, "*", SearchOption.AllDirectories);
                foreach (string dir in dirList)
                {
                    string dirName = dir.Substring(dir.LastIndexOf('/') + 1, dir.Length - dir.LastIndexOf('/') - 1);
                    if (!Directory.Exists(fullBackupFolderPath + "/" + dirName))
                    {
                        //create the directory
                        Directory.CreateDirectory(fullBackupFolderPath + "/" + dirName);
                    }
                }

                //bit of a re write to hit all directories
                var allFiles = Directory.GetFiles(workingFilesPath, "*.*", SearchOption.AllDirectories);

                foreach (string newPath in allFiles)
                { 
                    File.Copy(newPath, newPath.Replace(workingFilesPath, fullBackupFolderPath + "/"), true);
                }


                //close the window
                Close();
            } else
            {
                bkButtonBackup.IsEnabled=false;
            }
            

        }

        private void bkCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
