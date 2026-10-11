using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> QuickIterateTexts = new Dictionary<string, string[]>
        {
            {
                "QuickIterate.Notice",
                new[]
                {
                    "WARUDO가 켜져 있으면 덮어쓴 Prop을 WARUDO가 스스로 다시 불러옵니다(보통 15초 안). 위치·크기 같은 WARUDO 쪽 설정은 남고, 프롭 안에서 바꾼 실행 중 값은 처음 값으로 돌아갑니다.",
                    "If WARUDO is running, it reloads the overwritten Prop by itself (usually within 15 seconds). Settings on the WARUDO side, such as position and size, are kept; values changed inside the prop while running go back to their initial values.",
                    "WARUDO が起動していれば、上書きした Prop を WARUDO が自動で読み込み直します(通常 15 秒以内)。位置やサイズなど WARUDO 側の設定は残り、プロップ内で実行中に変えた値は初期値に戻ります。"
                }
            },
        };
    }
}
