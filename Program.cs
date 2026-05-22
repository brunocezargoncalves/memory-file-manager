using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics.Tracing;

namespace MemoryFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("##### MEMORY FILE ORGANIZER #####\n");
            Console.WriteLine("Organize automaticamente imagens (*.png, *.jpeg, .jpg, .gif) em pastas estruturadas por ano e mês, com base no nome dos arquivos.");
            Console.WriteLine("Exemplo: o arquivo \"../2021-09-27 15.33.04.jpg\" será organizado para \"../2021/09 - Setembro/2021-09-27 15.33.04.jpg\".\n\n");

            Console.WriteLine("Passo 01");
            Console.WriteLine("Especifique o diretório que contém os arquivos a serem organizados. Exemplo: C:\\Users\\bruno\\...\\Fotos de Família: ");
            string? readingDirectory = Console.ReadLine();

            Console.WriteLine("\nPasso 02");
            Console.WriteLine("Especifique o diretório onde os arquivos serão organizados. Exemplo: C:\\Users\\bruno\\...\\Fotos");
            string? writingDirectory = Console.ReadLine();
            
            if (!String.IsNullOrWhiteSpace(readingDirectory) && !String.IsNullOrWhiteSpace(writingDirectory))
            {
                string logFilePath = $"{writingDirectory}\\Log_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";

                Console.WriteLine("\nPasso 03");
                Console.WriteLine(Log("Organizando os arquivos....", logFilePath));

                if (Directory.Exists(readingDirectory))
                {
                    // Extensões de arquivo a serem listadas
                    string[] extensions = new[] { "*.png", "*.jpeg", "*.jpg", "*.gif", "*.mp4", "*.3gp" };

                    // Percorra cada extensão e liste os arquivos do diretório pai
                    foreach (var extension in extensions)
                    {
                        string[] files = Directory.GetFiles(readingDirectory, extension, SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            string fileName = Path.GetFileName(file);

                            // Identificar se o nome do arquivo corresponde ao padrão esperado: "AAA_YYYYMMDD_HHMMSS.EXT" (exemplo: "IMG_20210927_153304.jpg")
                            //string pattern = @"^[A-Za-z]{3}_[0-9]{8}_[0-9]{6}\.[A-Za-z]{3}$";
                            string pattern = @"^[A-Za-z]{3}_[0-9]{8}_[0-9]{6}\.[A-Za-z0-9]+$";

                            bool isValid = Regex.IsMatch(fileName, pattern);

                            if (isValid)
                            {
                                try
                                {
                                    Console.WriteLine(Log($"Arquivo {fileName} organizado para {MoveFile(Path.Combine(CreateMonthDirectory(CreateYearDirectory(writingDirectory, fileName), fileName), fileName), file)}.", logFilePath));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(Log(ex.Message, logFilePath));
                                }
                            }
                            else
                            {
                                Console.WriteLine("O texto NÃO corresponde ao padrão esperado.");

                                //string extension = Path
                                //.GetExtension(filePath)
                                //.ToLower();

                                //if (extension == ".jpg" ||
                                //    extension == ".jpeg" ||
                                //    extension == ".png")
                                //{
                                //    string imageDate = ExtractImageDate(filePath);

                                //    if (!String.IsNullOrWhiteSpace(imageDate))
                                //        return imageDate;
                                //}

                                //if (extension == ".mp4" ||
                                //    extension == ".3gp")
                                //{
                                //    string videoDate = ExtractVideoDate(filePath);

                                //    if (!String.IsNullOrWhiteSpace(videoDate))
                                //        return videoDate;
                                //}

                                //return File
                                //    .GetCreationTime(filePath)
                                //    .ToString("yyyyMMdd");
                            }                                                    
                        }
                    }

                    Console.WriteLine(Log("Organização de arquivos concluída.", logFilePath));
                    Console.WriteLine(Log("Excluindo diretórios vazios dos arquivos organizados....", logFilePath));
                    DeleteEmptyFolders(readingDirectory);
                    Console.WriteLine(Log("Organização dos arquivos concluída!", logFilePath));
                }
                else
                {
                    Console.WriteLine(Log($"Não foi encontrado o diretório {readingDirectory}.", logFilePath));
                }
            }
            else
            {
                Console.WriteLine($"Não foi possível identificar os diretórios. Os caminhos dos diretórios não foram fornecidos e/ou são inválidos.");
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static string CreateYearDirectory(string sourceDirectory, string fileName)
        {
            try
            {
                if (!String.IsNullOrEmpty(sourceDirectory) && !String.IsNullOrEmpty(fileName))
                {
                    string dateOnlyNumbers = Regex.Replace(fileName, @"\D", "");

                    int year = ExtractYearOrMonthFileName(dateOnlyNumbers.Length >= 8 ? dateOnlyNumbers[..8] : dateOnlyNumbers, "YEAR"); 

                    if (year > 1996 && year <= 2026)
                    {
                        string yearDirectory = Path.Combine(sourceDirectory, year.ToString());
                        Directory.CreateDirectory(yearDirectory);

                        return yearDirectory;
                    }
                    else
                    {
                        throw new Exception($"O nome do arquivo não corresponde ao padrão esperado.");
                    }
                }
                else
                {
                    throw new Exception($"O diretório de origem e/ou o nome do arquivo não foram fornecidos ou são inválidos.");
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Não foi possível criar ou definir o diretório ANO para organizar o arquivo {fileName}. {ex.Message}");
            }
        }

        static string CreateMonthDirectory(string yearDirectory, string fileName)
        {
            try
            {
                if (!String.IsNullOrEmpty(yearDirectory) && !String.IsNullOrEmpty(fileName))
                {
                    string dateOnlyNumbers = Regex.Replace(fileName, @"\D", "");

                    int month = ExtractYearOrMonthFileName(dateOnlyNumbers.Length >= 8 ? dateOnlyNumbers[..8] : dateOnlyNumbers, "MONTH");

                    string[] months = new string[]
                    {
                    "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
                    };

                    if (month >= 1 && month <= 12)
                    {
                        string monthDirectory = Path.Combine(yearDirectory, $"{month:D2} - {months[month - 1]}");
                        Directory.CreateDirectory(monthDirectory);

                        return monthDirectory;
                    }
                    else
                    {
                        throw new Exception($"O nome do arquivo não corresponde ao padrão esperado.");
                    }
                }
                else
                {
                    throw new Exception($"O diretório de origem e/ou o nome do arquivo não foram fornecidos ou são inválidos.");
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Não foi possível criar ou definir o diretório MÊS para organizar o arquivo {fileName}. {ex.Message}");
            }
        }

        static string MoveFile(string destinationFilePath, string sourceFilePath)
        {
            try
            {
                if (File.Exists(sourceFilePath))
                {
                    if (File.Exists(destinationFilePath))
                    {                        
                        File.Delete(destinationFilePath);
                    }

                    File.Move(sourceFilePath, destinationFilePath);
                    return destinationFilePath;
                }
                else
                {
                    throw new Exception($"O arquivo de origem ({sourceFilePath}) não foi encontrado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível organizar o arquivo. {ex.Message}");
            }
        }

        static void DeleteEmptyFolders(string readingDirectory)
        {
            try
            {
                foreach (var subdirectory in Directory.GetDirectories(readingDirectory))
                {
                    DeleteEmptyFolders(subdirectory);
                }

                if (Directory.GetFiles(readingDirectory).Length == 0 && Directory.GetDirectories(readingDirectory).Length == 0)
                {
                    Directory.Delete(readingDirectory);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível excluir pastas vazias do diretório {readingDirectory}: {ex.Message}");
            }
        }

        static int ExtractYearOrMonthFileName(string dateString, string yearOrMonth)
        {
            // YYYYMMDD
            string pattern = @"^\d{4}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])$";

            if (Regex.IsMatch(dateString, pattern))
            {
                if (DateTime.TryParseExact(dateString, "yyyyMMdd", null, DateTimeStyles.None, out DateTime validDate))
                {
                    if (yearOrMonth == "YEAR")
                    {
                        return validDate.Year;
                    }
                    if (yearOrMonth == "MONTH")
                    {
                        return validDate.Month;
                    }
                }
            }

            return 0;
        }        

        static string Log(string message, string logFilePath)
        {
            string logMessage = $"{DateTime.Now} - {message}";

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(logMessage);
                    return logMessage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível gravar no arquivo de log. {ex.Message}");
            }
        }
    }
}