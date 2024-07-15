using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;

namespace MinhaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadiosController : ControllerBase
    {
        private readonly MyDbContext _context;

        public EstadiosController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estadio>>> GetEstadios()
        {
            return await _context.Estadios.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Exame25")]
        public async Task<ActionResult<Estadio>> GetEstadio(int id)
        {
            var estadio = await _context.Estadios.FindAsync(id);

            if (estadio == null)
            {
                return NotFound();
            }

            return estadio;
        }

        [HttpPost]
        [Authorize(Roles = "Exame25")]
        public async Task<ActionResult<Estadio>> PostEstadio(Estadio estadio)
        {
            _context.Estadios.Add(estadio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEstadio", new { id = estadio.Id }, estadio);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Exame25")]
        public async Task<IActionResult> PutEstadio(int id, Estadio estadio)
        {
            if (id != estadio.Id)
            {
                return BadRequest();
            }

            _context.Entry(estadio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Exame25")]
        public async Task<IActionResult> DeleteEstadio(int id)
        {
            var estadio = await _context.Estadios.FindAsync(id);
            if (estadio == null)
            {
                return NotFound();
            }

            _context.Estadios.Remove(estadio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstadioExists(int id)
        {
            return _context.Estadios.Any(e => e.Id == id);
        }

        [HttpGet("export")]
        [Authorize(Roles = "Exame25")]
        public IActionResult ExportToXml()
        {
            try
            {
                var estadios = _context.Estadios.ToList();

                var xmlData = new XElement("Estadios",
                    estadios.Select(e => new XElement("Estadio",
                        new XAttribute("Id", e.Id),
                        new XElement("Nome", e.Nome),
                        new XElement("Capacidade", e.Capacidade),
                        new XElement("Morada", e.Morada),
                        new XElement("Cidade", e.Cidade))));

                var xmlString = xmlData.ToString();

                var byteArray = Encoding.UTF8.GetBytes(xmlString);
                var stream = new MemoryStream(byteArray);

                return File(stream, "application/xml", "estadios.xml");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao exportar estádios para XML: {ex.Message}");
            }
        }


        [HttpPost("importCsv")]
        [Authorize(Roles = "Exame25")]
        public async Task<IActionResult> ImportEstadiosFromCsv()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo enviado.");
                }

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var headers = await reader.ReadLineAsync();

                    List<Estadio> newEstadios = new List<Estadio>();
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var values = line.Split(',');

                        if (values.Length < 4)
                        {
                            return BadRequest("O formato das linhas no arquivo CSV não está correto.");
                        }

                        var estadio = new Estadio
                        {
                            Nome = values[0].Trim(),
                            Morada = values[2].Trim(),
                            Cidade = values[3].Trim()
                        };

                        if (int.TryParse(values[1].Trim(), out int capacidade))
                        {
                            estadio.Capacidade = capacidade;
                        }
                        else
                        {
                            return BadRequest($"Valor inválido para capacidade: {values[1].Trim()}. Certifique-se de que a capacidade é um número inteiro válido.");
                        }

                        newEstadios.Add(estadio);
                    }

                    _context.Estadios.AddRange(newEstadios);
                    await _context.SaveChangesAsync();

                    return Ok("Importação de estádios concluída com sucesso!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao importar estádios do CSV: {ex.Message}");
            }
        }

        [HttpPost("importXml")]
        [Authorize(Roles = "Exame25")]
        public async Task<IActionResult> ImportEstadiosFromXml()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo enviado.");
                }

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var xmlData = await reader.ReadToEndAsync();

                    var xml = XElement.Parse(xmlData);
                    var newEstadios = xml.Elements("Estadio").Select(e => new Estadio
                    {
                        Nome = e.Element("Nome")?.Value,
                        Capacidade = int.Parse(e.Element("Capacidade")?.Value ?? "0"),
                        Morada = e.Element("Morada")?.Value,
                        Cidade = e.Element("Cidade")?.Value
                    }).ToList();

                    _context.Estadios.AddRange(newEstadios);
                    await _context.SaveChangesAsync();

                    return Ok("Importação de estádios concluída com sucesso!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao importar estádios do XML: {ex.Message}");
            }
        }

    }
}
