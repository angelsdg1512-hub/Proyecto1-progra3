namespace ManagementSystem.DataAccess.Interfaces;

// Contrato común elegido para coincidir con los nombres del profesor.
// Sustituye la versión anterior Agregar/Actualizar/Eliminar.
public interface IRepository<T> where T : class
{
    List<T> GetAll();
    T? GetById(int id);
    int Create(T entidad); // ID generado por SQL Server.
    bool Update(T entidad); // true si se modificó una fila.
    bool Delete(int id);
}
