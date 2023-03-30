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
            if(DriveInfo.GetDrives() != null)
            {
                string driveDesc;
                string driveVolLable;

                var driveList = DriveInfo.GetDrives();
                foreach(var drive in driveList)
                {
                    if(drive.VolumeLabel != "")
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
            if(bkListBoxDrives.SelectedItem != null)
            {
                //make the filename
                DateTime currentDateTime = DateTime.Now;
                string bkfoldername = currentDateTime.ToString("ddMMyyy");
                bkfoldername = bkfoldername + "-";
                bkfoldername = bkfoldername + currentDateTime.ToString("HHmmss");

                string backupDrive = bkListBoxDrives.SelectedItem.ToString().Substring(0, 2);
                backupDrive = backupDrive + "/LaundryBackup";

                string fullBackupFolderPath = "";
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

                //should now have the full back made
                string workingFilesPath = "";
                workingFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\";
                //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\";

                //loop each directory
                string[] dirs = Directory.GetDirectories(workingFilesPath, "*", SearchOption.AllDirectories);
                string shortDir = "";
                string shortFile = "";
                foreach (string dir in dirs)
                {
                    //create the directory in the backup faolder
                    //get the directory name
                    for(int chars = dir.Length-1; chars > 0; chars--)
                    {
                        if(dir[chars] == '\\')
                        {
                            shortDir = dir.Substring(chars);
                            break;
                        }
                    }
                    Directory.CreateDirectory(fullBackupFolderPath + shortDir);
                    string[] files = Directory.GetFiles(dir);
                    foreach (string file in files)
                    {
                        for (int chars = file.Length - 1; chars > 0; chars--)
                        {
                            if (file[chars] == '\\')
                            {
                                shortFile = file.Substring(chars);
                                break;
                            }
                        }
                        File.Copy(file, fullBackupFolderPath + shortDir + shortFile, true);
                    }
                }

                //copy the top level files as well
                string[] topfiles = Directory.GetFiles(workingFilesPath);
                foreach (string file in topfiles)
                {
                    for (int chars = file.Length - 1; chars > 0; chars--)
                    {
                        if (file[chars] == '\\')
                        {
                            shortFile = file.Substring(chars);
                            break;
                        }
                    }
                    File.Copy(file, fullBackupFolderPath + shortFile, true);
                }


                //close the window
                Close();
            } else
            {
                bkButtonBackup.IsEnabled=false;
            }
            

        }


    }
}
