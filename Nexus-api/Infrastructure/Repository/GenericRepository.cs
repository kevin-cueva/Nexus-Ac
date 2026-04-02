using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nexus_api.Infrastructure.Data;

namespace Nexus_api.Infrastructure.Repository;

/// <summary>
/// Operaciones CRUD genericas aplicables en los modelos ORM.
/// </summary>
/// <typeparam name="TModelo"></typeparam>
public class GenericRepository<TModelo>(NexusDbContext dbcontext) : IGenericRepository<TModelo>
    where TModelo : class
{
    protected readonly NexusDbContext _dbcontexto = dbcontext;

    /// <summary>
    /// Consultar registro de la tabla en BD con filtro dinamico opcional.
    /// </summary>
    /// <param name="filtro"></param>
    /// <returns></returns>
    public async Task<TModelo?> Obtener(Expression<Func<TModelo, bool>> filtro)
    {
        TModelo? modelo = await _dbcontexto.Set<TModelo>().FirstOrDefaultAsync(filtro);
        return modelo;
    }

    /// <summary>
    /// Crear un registro de la tabla en BD.
    /// </summary>
    /// <param name="modelo"></param>
    /// <returns>Estado de la operación</returns>
    public async Task<TModelo> Crear(TModelo modelo)
    {
        _dbcontexto.Set<TModelo>().Add(modelo);
        await _dbcontexto.SaveChangesAsync();
        return modelo;
    }

    /// <summary>
    /// Editar o actualizar registro de la tabla en BD.
    /// </summary>
    /// <param name="modelo"></param>
    /// <returns>Estado de la operación</returns>
    public async Task Editar(TModelo modelo)
    {
        _dbcontexto.Set<TModelo>().Update(modelo);
        await _dbcontexto.SaveChangesAsync();
    }

    /// <summary>
    /// Eliminar registro de la tabla en BD.
    /// </summary>
    /// <param name="modelo"></param>
    /// <returns>Estado de la operación</returns>
    public async Task Eliminar(TModelo modelo)
    {
        _dbcontexto.Set<TModelo>().Remove(modelo);
        await _dbcontexto.SaveChangesAsync();
    }

    /// <summary>
    /// Consulta coleccion o tabla en la BD de forma generica reutilizable, Prepara la consulta, para quien llame el metodo, sea quien la ejecute.
    /// </summary>
    /// <param name="filtro">Filtro dinamico opcional para los registros</param>
    /// <returns>Retorna coleccion de registros</returns>
    public IQueryable<TModelo> Consultar(Expression<Func<TModelo, bool>>? filtro = null)
    {
        IQueryable<TModelo> queryModelo =
            filtro == null ? _dbcontexto.Set<TModelo>() : _dbcontexto.Set<TModelo>().Where(filtro);
        return queryModelo;
    }

    /// <summary>
    /// Crea una coleccion de registros
    /// </summary>
    /// <param name="modelo"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TModelo>> CrearVarios(IEnumerable<TModelo> modelo)
    {
        _dbcontexto.Set<TModelo>().AddRange(modelo);
        await _dbcontexto.SaveChangesAsync();
        return modelo;
    }

    /// <summary>
    /// Actualiza una colección de entidades a la vez.
    /// Si alguna entidad tiene un valor por defecto en su clave primaria, se creará una nueva.
    /// </summary>
    /// <param name="modelos">Colección de modelos con los nuevos valores</param>
    /// <returns>Las entidades actualizadas o creadas</returns>
    public async Task<IEnumerable<TModelo>> ActualizarInsertarVarios(IEnumerable<TModelo> modelos)
    {
        _dbcontexto.UpdateRange(modelos);
        await _dbcontexto.SaveChangesAsync();
        return modelos;
    }
}

