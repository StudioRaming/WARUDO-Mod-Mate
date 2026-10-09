using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> FlowTexts = new Dictionary<string, string[]>
        {
            {
                "Ui.Live.ConnectedPid",
                new[]
                {
                    "연결된 WARUDO: PID {0}",
                    "Connected to WARUDO: PID {0}",
                    "接続先WARUDO: PID {0}"
                }
            },
            {
                "Ui.Live.DifferentPid",
                new[]
                {
                    "현재 연결은 PID {0}입니다. 선택한 PID {1}을 사용하려면 다시 연결하세요.",
                    "Connected to PID {0}. Reconnect to use selected PID {1}.",
                    "現在の接続先はPID {0}です。選択したPID {1}を使用するには再接続してください。"
                }
            },
            {
                "Ui.Live.Invalid",
                new[]
                {
                    "대상 사전 점검에서 발견된 문제를 해결하고 다시 점검하세요.",
                    "Fix the target inspection issues, then inspect it again.",
                    "対象の事前確認で見つかった問題を修正し、もう一度事前確認してください。"
                }
            },
            {
                "Ui.Live.Send",
                new[]
                {
                    "WARUDO로 보내기",
                    "Send to WARUDO",
                    "WARUDOに送信"
                }
            },
            {
                "Ui.Live.Resend",
                new[]
                {
                    "대상 다시 보내기",
                    "Resend this object",
                    "対象を再送信"
                }
            },
            {
                "Ui.Live.Resend.tip",
                new[]
                {
                    "메시·텍스처·구조가 바뀌었을 때 다시 보냅니다. 수치만 바뀌었다면 값 갱신을 사용하세요.",
                    "Resend after mesh, texture, or hierarchy changes. Use value sync for value-only changes.",
                    "メッシュ・テクスチャ・階層を変更した場合は再送信します。値だけの変更には値の更新を使用してください。"
                }
            },
            {
                "Ui.History.Manage",
                new[]
                {
                    "이력 정리 · 삭제",
                    "Manage and delete history",
                    "履歴の整理・削除"
                }
            },
            {
                "Ui.History.DeleteRecords",
                new[]
                {
                    "선택 기록 삭제",
                    "Delete selected records",
                    "選択した履歴を削除"
                }
            },
            {
                "Ui.History.ClearRecords",
                new[]
                {
                    "전체 기록 삭제",
                    "Clear all history",
                    "履歴をすべて削除"
                }
            },
            {
                "Ui.Library.ColumnName",
                new[]
                {
                    "모드 파일",
                    "Mod file",
                    "モッドファイル"
                }
            },
            {
                "Ui.Library.ColumnStatus",
                new[]
                {
                    "기록 상태",
                    "Record status",
                    "記録状態"
                }
            },
            {
                "Ui.Library.ColumnVersion",
                new[]
                {
                    "버전",
                    "Version",
                    "バージョン"
                }
            },
            {
                "Ui.Library.ColumnSize",
                new[]
                {
                    "용량",
                    "Size",
                    "サイズ"
                }
            },
            {
                "Ui.Library.ColumnModified",
                new[]
                {
                    "수정 시각",
                    "Modified",
                    "更新日時"
                }
            },
            {
                "Ui.Library.StatusLegend",
                new[]
                {
                    "{0}: Mod Mate 빌드 기록 연결 · {1}: 연결된 기록 없음",
                    "{0}: Mod Mate build record linked · {1}: No linked record",
                    "{0}: Mod Mateビルド記録に接続 · {1}: 接続された記録なし"
                }
            },
            {
                "Ui.Library.SelectionCue",
                new[]
                {
                    "{0}개 선택 · 기록 연결 {1}개 · 외부 파일 {2}개",
                    "Selected: {0} · Linked records: {1} · External files: {2}",
                    "選択: {0}件 · 記録に接続: {1}件 · 外部ファイル: {2}件"
                }
            },
            {
                "Ui.History.ColumnStatus",
                new[]
                {
                    "결과",
                    "Result",
                    "結果"
                }
            },
            {
                "Ui.History.ColumnName",
                new[]
                {
                    "빌드 이름",
                    "Build name",
                    "ビルド名"
                }
            },
            {
                "Ui.History.ColumnFile",
                new[]
                {
                    "파일 상태",
                    "File status",
                    "ファイル状態"
                }
            },
            {
                "Ui.History.ColumnType",
                new[]
                {
                    "종류",
                    "Type",
                    "種類"
                }
            },
            {
                "Ui.History.ColumnVersion",
                new[]
                {
                    "버전",
                    "Version",
                    "バージョン"
                }
            },
            {
                "Ui.History.ColumnAuthor",
                new[]
                {
                    "제작자",
                    "Author",
                    "作者"
                }
            },
            {
                "Ui.History.ColumnDuration",
                new[]
                {
                    "소요 시간",
                    "Duration",
                    "所要時間"
                }
            },
            {
                "Ui.History.ColumnSize",
                new[]
                {
                    "용량",
                    "Size",
                    "サイズ"
                }
            },
            {
                "Ui.History.ColumnTime",
                new[]
                {
                    "빌드 시각",
                    "Build time",
                    "ビルド日時"
                }
            },
            {
                "Ui.History.SelectionCue",
                new[]
                {
                    "{0}개 선택",
                    "{0} selected",
                    "{0}件選択"
                }
            },
            {
                "Build.SourceSection",
                new[]
                {
                    "빌드할 프리팹 선택",
                    "Select prefabs to build",
                    "ビルドするプレハブを選択"
                }
            },
            {
                "Build.SourceSection.tip",
                new[]
                {
                    "빌드할 프리팹을 목록에서 고릅니다. 클릭은 하나, Ctrl+클릭은 추가/해제, Shift+클릭은 범위 선택입니다.",
                    "Choose the prefabs to build from the list. Click selects one; Ctrl-click toggles; Shift-click selects a range.",
                    "一覧からビルドするプレハブを選びます。クリックで1つ、Ctrl+クリックで追加/解除、Shift+クリックで範囲選択です。"
                }
            },
            {
                "Ui.Task.Build",
                new[]
                {
                    "프리팹 선택 → 출력 설정 → 선택한 대상 빌드",
                    "Select a prefab → Configure output → Build the selected target",
                    "プレハブを選択 → 出力を設定 → 選択した対象をビルド"
                }
            },
            {
                "Ui.Task.Library",
                new[]
                {
                    "만들어진 모드 파일을 확인하고, 원본이 연결된 모드를 다시 빌드합니다.",
                    "Review created mod files and rebuild mods linked to their sources.",
                    "作成したモッドファイルを確認し、元のソースに接続されたモッドを再ビルドします。"
                }
            },
            {
                "Ui.Task.History",
                new[]
                {
                    "빌드 결과와 실패 원인을 확인하고, 필요한 작업을 다시 실행합니다.",
                    "Review build results and failure causes, then rerun the needed actions.",
                    "ビルド結果と失敗原因を確認し、必要な作業を再実行します。"
                }
            },
            {
                "Ui.Task.Live",
                new[]
                {
                    "Unity의 오브젝트를 WARUDO로 보내고, 바뀐 값을 바로 확인합니다.",
                    "Send Unity objects to WARUDO and see changed values immediately.",
                    "UnityのオブジェクトをWARUDOへ送り、変更した値をすぐに確認します。"
                }
            },
            {
                "Ui.Live.Connection",
                new[]
                {
                    "1. WARUDO 연결",
                    "1. Connect to WARUDO",
                    "1. WARUDOに接続"
                }
            },
            {
                "Ui.Live.Target",
                new[]
                {
                    "2. 테스트할 대상",
                    "2. Test target",
                    "2. テスト対象"
                }
            },
            {
                "Ui.Live.Transfer",
                new[]
                {
                    "3. 보내기와 값 갱신",
                    "3. Send and update values",
                    "3. 送信と値の更新"
                }
            },
            {
                "Ui.Live.NoProcess",
                new[]
                {
                    "WARUDO를 실행하면 연결할 인스턴스가 표시됩니다.",
                    "Start WARUDO to see the available instances.",
                    "WARUDOを起動すると接続可能なインスタンスが表示されます。"
                }
            },
            {
                "Ui.Live.NeedsConnection",
                new[]
                {
                    "선택한 WARUDO 인스턴스의 수신기를 먼저 연결하세요.",
                    "Connect the receiver for the selected WARUDO instance first.",
                    "選択したWARUDOインスタンスの受信機を先に接続してください。"
                }
            },
            {
                "Ui.Live.NeedsObject",
                new[]
                {
                    "보낼 오브젝트를 지정하거나 현재 선택을 가져오세요.",
                    "Specify an object to send or use the current selection.",
                    "送信するオブジェクトを指定するか、現在の選択を使用してください。"
                }
            },
            {
                "Ui.Live.Waiting",
                new[]
                {
                    "이전 작업의 응답을 기다리고 있습니다.",
                    "Waiting for the previous operation's response.",
                    "前の操作の応答を待っています。"
                }
            },
            {
                "Ui.Live.Advanced",
                new[]
                {
                    "커스텀 코드 전송 · 고급",
                    "Custom code transfer · Advanced",
                    "カスタムコード転送 · 詳細"
                }
            },
            {
                "Ui.Live.ActiveTarget",
                new[]
                {
                    "현재 전송된 대상: {0} · {1}",
                    "Active target: {0} · {1}",
                    "送信済みの対象: {0} · {1}"
                }
            },
            {
                "Ui.Live.NoBinding",
                new[]
                {
                    "대상을 전송한 뒤 값 갱신과 테스트 자산 제거를 사용할 수 있습니다.",
                    "After sending a target, you can update values and remove the test asset.",
                    "対象を送信すると、値の更新とテストアセットの削除を使えるようになります。"
                }
            },
            {
                "Ui.Live.Preparing",
                new[]
                {
                    "전송할 데이터를 준비하고 있습니다.",
                    "Preparing the data to send.",
                    "送信するデータを準備しています。"
                }
            },
        };
    }
}
