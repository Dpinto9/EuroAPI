using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        string token = null;

        while (true)
        {
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Obter dados das Contas");
            Console.WriteLine("3. Obter dados dos Estádios");
            Console.WriteLine("4. Adicionar Estádio");
            Console.WriteLine("5. Atualizar Estádio");
            Console.WriteLine("6. Excluir Estádio");
            Console.WriteLine("7. Importar Estádios (CSV)");
            Console.WriteLine("8. Exportar Estádios (XML)");
            Console.WriteLine("9. Importar Estádios (XML)");
            Console.WriteLine("10. Limpar a Consola");
            Console.WriteLine("11. Sair");
            Console.WriteLine("---------------------------------------");

            Console.Write("Escolher uma opção: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    token = await Login(client);
                    break;
                case "2":
                    await GetData(client, token, "getDataWithoutRole");
                    break;
                case "3":
                    await GetData(client, token, "getDataWithRole");
                    break;
                case "4":
                    await AddEstadio(client, token);
                    break;
                case "5":
                    await UpdateEstadio(client, token);
                    break;
                case "6":
                    await DeleteEstadio(client, token);
                    break;
                case "7":
                    await ImportEstadios(client, token);
                    break;
                case "8":
                    await ExportEstadios(client, token);
                    break;
                case "9":
                    await ImportEstadiosFromXml(client, token);
                    break;
                case "10":
                    Console.Clear();
                    break;
                case "11":
                    return;
            }
        }
    }

    static async Task<string> Login(HttpClient client)
    {
        var user = new { Username = "Oportunidade", Password = "junho25", Role = "" };
        var response = await client.PostAsJsonAsync("https://localhost:7261/api/Is/login", user);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Console.WriteLine("Login successful!");
            return data?.Token;
        }
        Console.WriteLine("Login failed!");
        return null;
    }

    static async Task GetData(HttpClient client, string token, string endpoint)
    {
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"https://localhost:7261/api/Is/{endpoint}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Data: {data}");
        }
        else
        {
            Console.WriteLine("Request failed!");
        }
    }

    static async Task AddEstadio(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        Console.Write("Nome: ");
        var nome = Console.ReadLine();

        int capacidade;
        while (true)
        {
            Console.Write("Capacidade: ");
            if (int.TryParse(Console.ReadLine(), out capacidade))
                break;
            Console.WriteLine("Capacidade inválida. Por favor, insira um número inteiro válido.");
        }

        Console.Write("Morada: ");
        var morada = Console.ReadLine();
        Console.Write("Cidade: ");
        var cidade = Console.ReadLine();

        var estadio = new { Nome = nome, Capacidade = capacidade, Morada = morada, Cidade = cidade };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("https://localhost:7261/api/Estadios", estadio);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Estádio adicionado com sucesso!");
        }
        else
        {
            Console.WriteLine("Falha ao adicionar estádio!");
        }
    }

    static async Task UpdateEstadio(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        Console.Write("ID do Estádio a ser atualizado: ");
        var id = int.Parse(Console.ReadLine());

        try
        {
            // Verifica se o estádio existe
            var checkResponse = await client.GetAsync($"https://localhost:7261/api/Estadios/{id}");
            if (!checkResponse.IsSuccessStatusCode)
            {
                if (checkResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Estádio com ID {id} não encontrado.");
                    return;
                }
                else
                {
                    Console.WriteLine($"Erro ao verificar estádio com ID {id}. Status Code: {checkResponse.StatusCode}");
                    return;
                }
            }

            Console.Write("Novo Nome: ");
            var nome = Console.ReadLine();

            int capacidade;
            while (true)
            {
                Console.Write("Nova Capacidade: ");
                if (int.TryParse(Console.ReadLine(), out capacidade))
                    break;
                Console.WriteLine("Capacidade inválida. Por favor, insira um número inteiro válido.");
            }

            Console.Write("Nova Morada: ");
            var morada = Console.ReadLine();
            Console.Write("Nova Cidade: ");
            var cidade = Console.ReadLine();

            var estadio = new { Id = id, Nome = nome, Capacidade = capacidade, Morada = morada, Cidade = cidade };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PutAsJsonAsync($"https://localhost:7261/api/Estadios/{id}", estadio);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Estádio atualizado com sucesso!");
            }
            else
            {
                Console.WriteLine("Falha ao atualizar estádio!");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erro de HTTP ao tentar verificar/atualizar estádio: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao tentar verificar/atualizar estádio: {ex.Message}");
        }
    }

    static async Task DeleteEstadio(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        Console.Write("ID do Estádio a ser excluído: ");
        var id = int.Parse(Console.ReadLine());

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.DeleteAsync($"https://localhost:7261/api/Estadios/{id}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Estádio excluído com sucesso!");
        }
        else
        {
            Console.WriteLine("Falha ao excluir estádio!");
        }
    }

    static async Task ImportEstadios(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        Console.Write("Caminho do arquivo CSV: ");
        var filePath = Console.ReadLine();

        try
        {
            var csvData = File.ReadAllText(filePath);

            using (var formData = new MultipartFormDataContent())
            using (var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvData)))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

                formData.Add(fileContent, "file", Path.GetFileName(filePath));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync("https://localhost:7261/api/Estadios/importCsv", formData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Importação de estádios concluída com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Falha ao importar estádios! Status Code: {response.StatusCode}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Detalhes do erro: {responseContent}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo CSV: {ex.Message}");
        }
    }


    static async Task ImportEstadiosFromXml(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        Console.Write("Caminho do arquivo XML: ");
        var filePath = Console.ReadLine();

        try
        {
            var xmlData = File.ReadAllText(filePath);

            using (var formData = new MultipartFormDataContent())
            using (var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlData)))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

                formData.Add(fileContent, "file", Path.GetFileName(filePath));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync("https://localhost:7261/api/Estadios/importXml", formData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Importação de estádios concluída com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Falha ao importar estádios! Status Code: {response.StatusCode}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Detalhes do erro: {responseContent}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo XML: {ex.Message}");
        }
    }


    static async Task ExportEstadios(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Por favor, faça login primeiro.");
            return;
        }

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var fileName = "EstadiosExport.xml";
        var filePath = Path.Combine(desktopPath, fileName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("https://localhost:7261/api/Estadios/export");

        if (response.IsSuccessStatusCode)
        {
            var xmlData = await response.Content.ReadAsStringAsync();
            File.WriteAllText(filePath, xmlData);
            Console.WriteLine($"Exportação de estádios concluída com sucesso! Arquivo salvo em: {filePath}");
        }
        else
        {
            Console.WriteLine("Falha ao exportar estádios!");
        }
    }
}
