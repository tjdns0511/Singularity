// In Assets/Scripts/IDataWithId.cs

/// <summary>
/// DataManager가 ScriptableObject를 ID(string) 기반 딕셔너리로
/// 관리할 수 있도록, 'ID' 프로퍼티를 갖도록 강제하는 인터페이스입니다.
/// </summary>
public interface IDataWithId
{
    /// <summary>
    /// DataManager가 딕셔너리의 Key로 사용할 고유 ID입니다.
    /// </summary>
    string ID { get; }
}