using System.Collections;

public interface ICoroutineEffect
{
    IEnumerator ApplyEffectRoutine(CharacterStats source, CharacterStats target);
}
