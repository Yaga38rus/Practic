using MVVM.Sample.Models;
using MVVM.Sample.Views;
using MySql.Data.MySqlClient;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Globalization;

namespace MVVM.Sample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=112;Database=Practica;";

        private inf_pc _selectedinf_pc;
        public inf_pc Selectedinf_pc
        {
            get => _selectedinf_pc;
            set
            {
                _selectedinf_pc = value;
                OnPropertyChanged(nameof(Selectedinf_pc));
                Execinf_soft_pcList();
                Readinf_freeList();
            }

        }

        private inf_software _selectedinf_software;
        public inf_software Selectedinf_software
        {
            get => _selectedinf_software;
            set
            {
                _selectedinf_software = value;
                OnPropertyChanged(nameof(Selectedinf_software));
            }

        }

        private inf_software _selectedfree;
        public inf_software Selectedfree
        {
            get => _selectedfree;
            set
            {
                _selectedfree = value;
                OnPropertyChanged(nameof(Selectedfree));
            }
        }

        private inf_soft_pc _selectedinf_soft_pc;
        public inf_soft_pc Selectedinf_soft_pc
        {
            get => _selectedinf_soft_pc;
            set
            {
                _selectedinf_soft_pc = value;
                OnPropertyChanged(nameof(Selectedinf_soft_pc));
            }

        }

        private ObservableCollection<inf_pc> _inf_pcList;
        public ObservableCollection<inf_pc> inf_pcList { get => _inf_pcList; set => _inf_pcList = value; }

        private ObservableCollection<inf_soft_pc> _inf_soft_pcList;
        public ObservableCollection<inf_soft_pc> inf_soft_pcList { get => _inf_soft_pcList; set => _inf_soft_pcList = value; }

        private ObservableCollection<inf_soft_pc> _inf_soft_freeList;
        public ObservableCollection<inf_soft_pc> inf_soft_freeList { get => _inf_soft_freeList; set => _inf_soft_freeList = value; }

        public MainViewModel()
        {
            inf_pcList = new ObservableCollection<inf_pc>();
            inf_soft_pcList = new ObservableCollection<inf_soft_pc>();
            inf_soft_freeList = new ObservableCollection<inf_soft_pc>();
            
            Readinf_pcList();
        }

        /// <summary>
        /// Метод чтения списка доступного ПО
        /// </summary>
        private void Readinf_freeList()
        {
            inf_pc exec_inf_pc = (inf_pc)Selectedinf_pc;

            if (exec_inf_pc == null) //проверка инициализации коллекции
                return;

            if (inf_soft_freeList == null) //проверка инициализации коллекции
                return;
            inf_soft_freeList.Clear(); //очищаем предыдущий вариант списка

            string CommandText = $"SELECT name_soft, num_license, inf_software.id_soft, COUNT(inf_soft_pc.id_soft) FROM inf_software LEFT JOIN inf_soft_pc ON inf_software.id_soft = inf_soft_pc.id_soft WHERE name_soft NOT IN (SELECT name_soft FROM inf_software INNER JOIN (inf_pc INNER JOIN inf_soft_pc ON inf_pc.id_pc= inf_soft_pc.id_pc) ON inf_software.id_soft = inf_soft_pc.id_soft WHERE name_pc = '{exec_inf_pc.name_pc}') GROUP BY name_soft, num_license, inf_software.id_soft ORDER BY name_soft ASC";

            NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
            NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
            try
            {
                dbConnection.Open();
                NpgsqlDataReader reader;
                reader = comm.ExecuteReader();

                while (reader.Read())  //чтение очередной записи запроса
                {
                    inf_soft_pc free = new inf_soft_pc()
                    {
                        name_soft = reader.GetString(0),
                        num_license=reader.GetInt32(1),
                        id_soft=reader.GetInt32(2),
                        count=reader.GetInt32(3)
                    };
                    inf_soft_freeList.Add(free); //добавление в коллекцию записей
                }
                reader.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            finally
            {
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Метод чтения списка сведений о ПК из базы
        /// </summary>
        private void Readinf_pcList()
        {
            if (inf_pcList == null) //проверка инициализации коллекции
                return;
            inf_pcList.Clear(); //очищаем предыдущий вариант списка

            string CommandText = "SELECT * FROM inf_pc";

            NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
            NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
            try
            {
                dbConnection.Open();
                NpgsqlDataReader reader;
                reader = comm.ExecuteReader();

                while (reader.Read())  //чтение очередной записи запроса
                {
                    inf_pc pc = new inf_pc()
                    {
                        id_pc = reader.GetInt32(0),
                        name_pc = reader.GetString(1),
                        tag = reader.GetInt32(2),
                        cpu = reader.GetString(3),
                        ram_gb = reader.GetInt32(4),
                        graphics_card = reader.GetString(5),
                        hdd_gb = reader.GetInt32(6)
                    };
                    inf_pcList.Add(pc); //добавление ПК в коллекцию записей
                }
                reader.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            finally
            {
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Метод чтения списка установленного ПО у конкретного ПК из базы
        /// </summary>
        private void Execinf_soft_pcList()
        {
            inf_pc exec_inf_pc = (inf_pc)Selectedinf_pc;

            if (exec_inf_pc == null)
                return;

            if (inf_soft_pcList == null) //проверка инициализации коллекции
                return;
            inf_soft_pcList.Clear(); //очищаем предыдущий вариант списка

            string CommandText = $"SELECT inf_software.id_soft, name_soft, inf_software.type, license, num_license FROM inf_soft_pc INNER JOIN inf_pc ON inf_soft_pc.id_pc=inf_pc.id_pc INNER JOIN inf_software ON inf_soft_pc.id_soft=inf_software.id_soft WHERE name_pc='{exec_inf_pc.name_pc}' ORDER BY name_soft ASC";

            NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
            NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
            try
            {
                dbConnection.Open();
                NpgsqlDataReader reader;
                reader = comm.ExecuteReader();

                while (reader.Read())  //чтение очередной записи запроса
                {
                    inf_soft_pc soft_pc = new inf_soft_pc()
                    {
                        id_soft = reader.GetInt32(0),
                        name_soft = reader.GetString(1),
                        type=reader.GetString(2),
                        license = reader.GetString(3),
                        num_license = reader.GetInt32(4)
                    };
                    inf_soft_pcList.Add(soft_pc); //добавление ПК в коллекцию записей
                }
                reader.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            finally
            {
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Команда на изменение ПК
        /// </summary>
        private RelayCommand _inf_pcEditCommand;
        public RelayCommand inf_pcEditCommand => _inf_pcEditCommand ??
                  (_inf_pcEditCommand = new RelayCommand((Selectedinf_pc) =>
                  {
                      if (Selectedinf_pc == null)
                      {
                          MessageBox.Show("Нужно выбрать ПК");
                          return;
                      }

                      inf_pc upd_inf_pc = (inf_pc)Selectedinf_pc;

                      var dialog = new inf_pcEditWindow()
                      {
                          DataContext = upd_inf_pc
                      };

                      if (dialog.ShowDialog() == true)
                      {
                          NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                          string CommandText = $"UPDATE inf_pc SET name_pc='{upd_inf_pc.name_pc}',tag='{upd_inf_pc.tag}',cpu='{upd_inf_pc.cpu}',ram_gb='{upd_inf_pc.ram_gb}',graphics_card='{upd_inf_pc.graphics_card}',hdd_gb='{upd_inf_pc.hdd_gb}' WHERE id_pc='{upd_inf_pc.id_pc}';";
                          NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                          try
                          {
                              dbConnection.Open();
                              comm.ExecuteNonQuery();
                              Readinf_pcList();
                          }
                          catch (Exception error)
                          {
                              MessageBox.Show(error.Message);
                          }
                          finally
                          {
                              dbConnection.Close();
                          }
                      }

                  }));

        /// <summary>
        /// Команда на добавление нового ПК
        /// </summary>
        private RelayCommand _inf_pcAddCommand;
        public RelayCommand inf_pcAddCommand => _inf_pcAddCommand ??
                  (_inf_pcAddCommand = new RelayCommand((commandparam) =>
                  {

                      inf_pc new_inf_pc = new inf_pc()
                      {

                      };

                      var dialog = new inf_pcEditWindow()
                      {
                          DataContext = new_inf_pc,
                      };

                      if (dialog.ShowDialog() == true)
                      {
                          NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                          //создание sql-запроса с парамтерами
                          string CommandText = "Insert Into inf_pc (name_pc,tag,cpu,ram_gb,graphics_card,hdd_gb) values (@name_pc,@tag,@cpu,@ram_gb,@graphics_card,@hdd_gb);";
                          NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                          comm.Parameters.AddWithValue("@name_pc", new_inf_pc.name_pc);
                          comm.Parameters.AddWithValue("@tag", new_inf_pc.tag);
                          comm.Parameters.AddWithValue("@cpu", new_inf_pc.cpu);
                          comm.Parameters.AddWithValue("@ram_gb", new_inf_pc.ram_gb);
                          comm.Parameters.AddWithValue("@graphics_card", new_inf_pc.graphics_card);
                          comm.Parameters.AddWithValue("@hdd_gb", new_inf_pc.hdd_gb);
                          try
                          {
                              dbConnection.Open();
                              comm.ExecuteNonQuery();
                              Readinf_pcList();
                              Readinf_freeList();
                              Execinf_soft_pcList();
                          }
                          catch (Exception error)
                          {
                              MessageBox.Show(error.Message);
                          }
                          finally
                          {
                              dbConnection.Close();
                          }

                      }

                  }));

        /// <summary>
        /// Команда на изменение ПО
        /// </summary>
        private RelayCommand _inf_softwareEditCommand;
        public RelayCommand inf_softwareEditCommand => _inf_softwareEditCommand ??
                  (_inf_softwareEditCommand = new RelayCommand((Selectedinf_soft_pc) =>
                  {
                      if (Selectedinf_soft_pc == null)
                      {
                          MessageBox.Show("Нужно выбрать установленное ПО");
                          return;
                      }

                      inf_soft_pc upd_inf_soft_pc = (inf_soft_pc)Selectedinf_soft_pc;

                      var dialog = new inf_softwareEditWindow()
                      {
                          DataContext = upd_inf_soft_pc
                      };

                      if (dialog.ShowDialog() == true)
                      {
                          NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                          string CommandText = $"UPDATE inf_software SET name_soft='{upd_inf_soft_pc.name_soft}',type='{upd_inf_soft_pc.type}',license='{upd_inf_soft_pc.license}',num_license='{upd_inf_soft_pc.num_license}' WHERE id_soft='{upd_inf_soft_pc.id_soft}';";
                          NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                          try
                          {
                              dbConnection.Open();
                              comm.ExecuteNonQuery();
                              Readinf_pcList();
                          }
                          catch (Exception error)
                          {
                              MessageBox.Show(error.Message);
                          }
                          finally
                          {

                              dbConnection.Close();
                          }

                      }

                  }));

        /// <summary>
        /// Команда на добавление нового ПО
        /// </summary>
        private RelayCommand _inf_softwareAddCommand;
        public RelayCommand inf_softwareAddCommand => _inf_softwareAddCommand ??
                  (_inf_softwareAddCommand = new RelayCommand((commandparam) =>
                  {

                      inf_software new_inf_software = new inf_software()
                      {

                      };

                      var dialog = new inf_softwareEditWindow()
                      {
                          DataContext = new_inf_software,
                      };

                      if (dialog.ShowDialog() == true)
                      {
                          NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                          //создание sql-запроса с парамтерами
                          string CommandText = "Insert Into inf_software (name_soft,type,license,num_license) values (@name_soft,@type,@license,@num_license);";
                          NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                          comm.Parameters.AddWithValue("@name_soft", new_inf_software.name_soft);
                          comm.Parameters.AddWithValue("@type", new_inf_software.type);
                          comm.Parameters.AddWithValue("@license", new_inf_software.license);
                          comm.Parameters.AddWithValue("@num_license", new_inf_software.num_license);
                          try
                          {
                              dbConnection.Open();
                              comm.ExecuteNonQuery();
                              Readinf_freeList();

                          }
                          catch (Exception error)
                          {
                              MessageBox.Show(error.Message);
                          }
                          finally
                          {
                              dbConnection.Close();
                          }
                      }

                  }));

        /// <summary>
        /// Команда на добавление ПК доступного ПО
        /// </summary>
        private RelayCommand _inf_soft_pcEditCommand;
        public RelayCommand inf_soft_pcEditCommand => _inf_soft_pcEditCommand ??
                  (_inf_soft_pcEditCommand = new RelayCommand((Selectedfree) =>
                  {
                      inf_soft_pc upd_inf_soft_pc = (inf_soft_pc)Selectedfree;
                      inf_pc new_inf_pc = (inf_pc)Selectedinf_pc;
                      inf_soft_pc license = (inf_soft_pc)Selectedfree;

                      if (Selectedinf_pc == null)
                      {
                          MessageBox.Show("Нужно выбрать ПК");
                          return;
                      }

                      if (Selectedfree == null)
                      {
                          MessageBox.Show("Нужно выбрать ПО");
                          return;
                      }

                      if (license.num_license == license.count)
                      {
                          MessageBox.Show("Свободных лицензий нет");
                          return;
                      }

                      NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                      string CommandText = "Insert Into inf_soft_pc (id_pc,id_soft) values (@id_pc,@id_soft);";
                      NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                      comm.Parameters.AddWithValue("@id_pc", new_inf_pc.id_pc);
                      comm.Parameters.AddWithValue("@id_soft", upd_inf_soft_pc.id_soft);
                      try
                      {
                          dbConnection.Open();
                          comm.ExecuteNonQuery();
                          Readinf_freeList();
                          Execinf_soft_pcList();
                      }
                      catch (Exception error)
                      {
                          MessageBox.Show(error.Message);
                      }
                      finally
                      {
                          dbConnection.Close();
                      }
                  }));

        /// <summary>
        /// Команда на удаление установленного ПО у ПК
        /// </summary>
        private RelayCommand _inf_soft_pcDelCommand;
        public RelayCommand inf_soft_pcDelCommand => _inf_soft_pcDelCommand ??
                  (_inf_soft_pcDelCommand = new RelayCommand((Selectedinf_software) =>
                  {
                      if (Selectedinf_software == null)
                      {
                          MessageBox.Show("Нужно выбрать ПО");
                          return;
                      }

                      if (Selectedinf_pc == null)
                      {
                          MessageBox.Show("Нужно выбрать ПК");
                          return;
                      }
                      inf_soft_pc del_inf_soft_pc = (inf_soft_pc)Selectedinf_software;
                      inf_pc new_inf_pc = (inf_pc)Selectedinf_pc;

                      NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
                      string CommandText = "DELETE FROM inf_soft_pc WHERE id_pc=@id_pc AND id_soft=@id_soft";
                      NpgsqlCommand comm = new NpgsqlCommand(CommandText, dbConnection);
                      comm.Parameters.AddWithValue("@id_pc", new_inf_pc.id_pc);
                      comm.Parameters.AddWithValue("@id_soft", del_inf_soft_pc.id_soft);
                      try
                      {
                          dbConnection.Open();
                          comm.ExecuteNonQuery();
                          Readinf_freeList();
                          Execinf_soft_pcList();
                      }
                      catch (Exception error)
                      {
                          MessageBox.Show(error.Message);
                      }
                      finally
                      {
                          dbConnection.Close();
                      }

                  }));

    }
}
