using System.Linq.Expressions;

namespace Nexus_api.Infrastructure.Repository;

public interface IGenericRepository<TModel>
    where TModel : class
{
    //Escribir todos los metodos que podemos trabajar con nuestros modelos, Contrato

    /// <summary>
    /// Obtiene una entidad que cumple con el filtro especificado.
    /// </summary>
    /// <param name="filtro">El filtro de expresión para buscar el modelo.</param>
    /// <returns>La entidad que cumple con el filtro o null si no se encuentra.</returns>
    Task<TModel?> Obtener(Expression<Func<TModel, bool>> filtro);

    /// <summary>
    /// Crea una nueva entidad.
    /// </summary>
    /// <param name="modelo">La entidad a crear.</param>
    /// <returns>La entidad creada.</returns>
    Task<TModel> Crear(TModel modelo);

    /// <summary>
    /// Edita una entidad existente.
    /// </summary>
    /// <param name="modelo">El modelo de la entidad a editar.</param>
    Task Editar(TModel modelo);

    /// <summary>
    /// Elimina una entidad existente.
    /// </summary>
    /// <param name="modelo">La instancia de la entidad a eliminar.</param>
    Task Eliminar(TModel modelo);

    /// <summary>
    /// Consulta las entidades que cumplen con el filtro especificado.
    /// </summary>
    /// <param name="filtro">El filtro de expresión para buscar las entidades.</param>
    /// <returns>Una consulta de entidades que cumplen con el filtro.</returns>
    IQueryable<TModel> Consultar(Expression<Func<TModel, bool>>? filtro = null);

    /// <summary>
    /// Crea varias entidades a la vez.
    /// </summary>
    /// <param name="modelo">Las entidades a crear.</param>
    /// <returns>Una colección de las entidades creados.</returns>
    Task<IEnumerable<TModel>> CrearVarios(IEnumerable<TModel> modelo);

    /// <summary>
    /// Actualiza una colección de entidades a la vez.
    /// Si alguna entidad tiene un valor por defecto en su clave primaria, se creará una nueva.
    /// </summary>
    /// <param name="modelos">Colección de modelos con los nuevos valores</param>
    /// <returns>Las entidades actualizadas o creadas</returns>
    Task<IEnumerable<TModel>> ActualizarInsertarVarios(IEnumerable<TModel> modelos);
}

