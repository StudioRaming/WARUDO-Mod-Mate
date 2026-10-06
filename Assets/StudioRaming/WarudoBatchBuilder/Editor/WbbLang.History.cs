using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> HistoryTexts = new Dictionary<string, string[]>
        {
            {
                "Guide.History",
                new[]
                {
                    "<b>1.</b> 빌드가 끝날 때마다 <b>어떤 프리팹을 · 어떤 버전으로 · 언제 · 성공했는지</b> 자동으로 기록됩니다. " + "따로 저장할 필요가 없습니다.\n" + "<b>2.</b> 위쪽 <b>타입 · 상태 · 검색</b> 으로 원하는 기록만 추립니다. " + "묶음 방식은 <b>날짜별 / 프리셋별 / 평면</b> 중에 고를 수 있고, 날짜 머리글을 클릭하면 접힙니다.\n" + "<b>3.</b> 행을 클릭하면 선택됩니다. <b>Ctrl+클릭</b> 은 하나씩 추가/해제, <b>Shift+클릭</b> 은 범위 선택, " + "<b>더블클릭</b> 은 소스 프리팹을 Project 창에서 찾아줍니다.\n" + "<b>4.</b> 행 우클릭(또는 행 오른쪽 <b>메뉴 버튼</b>)에서<b>재빌드 · 출력 폴더 열기 · 경로 복사 · 로그 보기 · 이 기록 삭제</b> 를 할 수 있습니다. " + "<b>파일 없음</b> 배지는 [새로고침] 때 산출물 파일을 찾지 못한 기록에만 붙습니다. " + "<b>자동 정리</b> 배지는 빌드용 복사본에서 정리한 것이 있다는 뜻이며, 마우스를 올리면 내역이 보입니다.\n" + "<b>5.</b> 아래쪽 <b>실패만 재빌드</b> 는 실패한 기록 중 프리팹이 같은 것은 가장 최근 1건만 골라냅니다. " + "기록이 쌓이면 <b>90일 이전 기록 정리</b> 로 줄이고, 필요하면 <b>CSV 내보내기</b> 로 백업하세요. " + "할 일이 없는 버튼(선택 0개 · 실패 0건 등)은 꺼져 있고, 마우스를 올리면 이유가 보입니다. 결과는 아래 상태 줄에 남습니다.",
                    "<b>1.</b> Every build is logged automatically: <b>which prefab, which version, when, and whether it succeeded</b>. " + "Nothing to save by hand.\n" + "<b>2.</b> Narrow the list with the <b>type / status / search</b> filters above. " + "Group by <b>date, preset or flat</b>; click a date header to collapse it.\n" + "<b>3.</b> Click a row to select it. <b>Ctrl+Click</b> toggles one, <b>Shift+Click</b> selects a range, " + "<b>Double-click</b> pings the source prefab in the Project window.\n" + "<b>4.</b> Right-click a row (or the <b>menu button</b> at the right of the row) for<b>rebuild, open output folder, copy path, view log, delete record</b>. " + "A <b>Missing</b> badge appears only on records whose output file was not found at the last [Refresh]. " + "A <b>Tidied</b> badge means something was cleaned up in the build copy; hover it to see what.\n" + "<b>5.</b> <b>Rebuild failed only</b> picks the newest failed record per prefab, without duplicates. " + "Use <b>Clean up old records</b> to trim the log, and <b>Export CSV</b> to back it up. " + "Buttons with nothing to do (no selection, no failures and so on) are off; hover one to see why. Results stay in the status line below.",
                    "<b>1.</b> ビルドのたびに <b>どのプレハブを・どのバージョンで・いつ・成功したか</b> が自動で記録されます。" + "手動保存は不要です。\n" + "<b>2.</b> 上部の <b>タイプ・状態・検索</b> で絞り込みます。" + "まとめ方は <b>日付別 / プリセット別 / フラット</b> から選べ、日付見出しをクリックすると折りたためます。\n" + "<b>3.</b> 行をクリックで選択。<b>Ctrl+クリック</b> で個別トグル、<b>Shift+クリック</b> で範囲選択、" + "<b>ダブルクリック</b> でソースプレハブを Project ウィンドウで表示します。\n" + "<b>4.</b> 行を右クリック(または行の右端の <b>メニューボタン</b>)で<b>再ビルド・出力フォルダを開く・パスをコピー・ログを見る・この記録を削除</b>。" + "<b>ファイルなし</b> バッジは、[更新] の時点で出力ファイルが見つからなかった記録にだけ付きます。" + "<b>自動整理</b> バッジはビルド用コピーで整理した内容があることを示し、マウスを乗せると内容が表示されます。\n" + "<b>5.</b> <b>失敗のみ再ビルド</b> は、同じプレハブの失敗記録から最新の1件だけを重複なく選びます。" + "記録が増えたら <b>90日より前の記録を整理</b>、必要なら <b>CSVエクスポート</b> でバックアップしてください。" + "することがないボタン(選択0件・失敗0件など)はオフになり、マウスを乗せると理由が表示されます。結果は下の状態行に残ります。"
                }
            },
            {
                "History.Stats",
                new[]
                {
                    "총 {0}건 · 성공 {1} · 실패 {2} · 그 외 {3}",
                    "{0} records · {1} succeeded · {2} failed · {3} other",
                    "合計 {0}件 · 成功 {1} · 失敗 {2} · その他 {3}"
                }
            },
            {
                "History.StatsExtra",
                new[]
                {
                    "최근 빌드 {0}",
                    "Last build {0}",
                    "最終ビルド {0}"
                }
            },
            {
                "History.Stats.tip",
                new[]
                {
                    "누적 산출 용량 {0}\n성공한 빌드가 남긴 산출물 크기를 모두 더한 값입니다.",
                    "Total output size {0}\nThe combined size of the files left by successful builds.",
                    "累計出力サイズ {0}\n成功したビルドが残した出力ファイルの合計サイズです。"
                }
            },
            {
                "History.Showing",
                new[]
                {
                    "{0}건 표시 / 전체 {1}건",
                    "Showing {0} of {1}",
                    "{0}件表示 / 全 {1}件"
                }
            },
            {
                "History.EmptyTitle",
                new[]
                {
                    "아직 빌드 이력이 없습니다",
                    "No build history yet",
                    "ビルド履歴はまだありません"
                }
            },
            {
                "History.EmptyBody",
                new[]
                {
                    "빌드할 때마다 무엇을 언제 빌드했는지 여기에 자동으로 기록됩니다.",
                    "Each build is recorded here automatically, with what was built and when.",
                    "ビルドするたびに、何をいつビルドしたかがここに自動で記録されます。"
                }
            },
            {
                "History.EmptyGoBuild",
                new[]
                {
                    "빌드 탭으로",
                    "Go to Build",
                    "ビルドタブへ"
                }
            },
            {
                "History.FilterType",
                new[]
                {
                    "타입",
                    "Type",
                    "タイプ"
                }
            },
            {
                "History.FilterType.tip",
                new[]
                {
                    "선택한 타입의 기록만 남깁니다.",
                    "Keep only records of the checked types.",
                    "選択したタイプの記録だけを表示します。"
                }
            },
            {
                "History.FilterStatus",
                new[]
                {
                    "상태",
                    "Status",
                    "状態"
                }
            },
            {
                "History.FilterStatus.tip",
                new[]
                {
                    "성공/실패/그 외(중단·건너뜀·대기) 중 볼 것을 고릅니다.",
                    "Choose which statuses to show: success, failed, or other (canceled/skipped/pending).",
                    "成功・失敗・その他(中断/スキップ/待機)から表示するものを選びます。"
                }
            },
            {
                "History.StatusOther",
                new[]
                {
                    "그 외",
                    "Other",
                    "その他"
                }
            },
            {
                "History.StatusOther.tip",
                new[]
                {
                    "중단·건너뜀·대기·진행 중 기록을 포함합니다.",
                    "Include canceled, skipped, pending and running records.",
                    "中断・スキップ・待機・実行中の記録を含めます。"
                }
            },
            {
                "History.Group",
                new[]
                {
                    "묶기",
                    "Group",
                    "まとめ"
                }
            },
            {
                "History.Group.tip",
                new[]
                {
                    "목록을 어떻게 묶어서 볼지 고릅니다.",
                    "Choose how rows are grouped.",
                    "行のまとめ方を選びます。"
                }
            },
            {
                "History.GroupDate",
                new[]
                {
                    "날짜별",
                    "By date",
                    "日付別"
                }
            },
            {
                "History.GroupDate.tip",
                new[]
                {
                    "yyyy-MM-dd 머리글로 묶습니다. 머리글을 클릭하면 접거나 펼칩니다.",
                    "Group under yyyy-MM-dd headers. Click a header to collapse or expand it.",
                    "yyyy-MM-dd の見出しでまとめます。見出しクリックで折りたたみ/展開。"
                }
            },
            {
                "History.GroupJob",
                new[]
                {
                    "프리셋별",
                    "By preset",
                    "プリセット別"
                }
            },
            {
                "History.GroupJob.tip",
                new[]
                {
                    "빌드에 사용한 프리셋(잡) 이름으로 묶습니다.",
                    "Group by the preset (job) used for the build.",
                    "ビルドに使ったプリセット(ジョブ)名でまとめます。"
                }
            },
            {
                "History.GroupFlat",
                new[]
                {
                    "평면",
                    "Flat",
                    "フラット"
                }
            },
            {
                "History.GroupFlat.tip",
                new[]
                {
                    "묶지 않고 최신순 한 줄 목록으로 봅니다.",
                    "No grouping. One flat list, newest first.",
                    "まとめずに新しい順の一覧で表示します。"
                }
            },
            {
                "History.GroupNoJob",
                new[]
                {
                    "(프리셋 없음)",
                    "(no preset)",
                    "(プリセットなし)"
                }
            },
            {
                "History.GroupHeader",
                new[]
                {
                    "{0}   {1}건 · 성공 {2} · 실패 {3}",
                    "{0}   {1} records · {2} ok · {3} failed",
                    "{0}   {1}件 · 成功 {2} · 失敗 {3}"
                }
            },
            {
                "History.FileFound",
                new[]
                {
                    "파일 있음",
                    "File OK",
                    "ファイルあり"
                }
            },
            {
                "History.FileMissing",
                new[]
                {
                    "파일 없음",
                    "Missing",
                    "ファイルなし"
                }
            },
            {
                "History.FileUnknown",
                new[]
                {
                    "미확인",
                    "Unchecked",
                    "未確認"
                }
            },
            {
                "History.TipLine1",
                new[]
                {
                    "타입 {0} · 버전 {1} · 제작자 {2}",
                    "Type {0} · version {1} · author {2}",
                    "タイプ {0} · バージョン {1} · 作者 {2}"
                }
            },
            {
                "History.TipLine2",
                new[]
                {
                    "상태 {0} · 시작 {1} · 소요 {2}",
                    "Status {0} · started {1} · took {2}",
                    "状態 {0} · 開始 {1} · 所要 {2}"
                }
            },
            {
                "History.TipLine3",
                new[]
                {
                    "산출물 {0} ({1})",
                    "Output {0} ({1})",
                    "出力 {0} ({1})"
                }
            },
            {
                "History.TipLine4",
                new[]
                {
                    "소스 프리팹 {0}",
                    "Source prefab {0}",
                    "ソースプレハブ {0}"
                }
            },
            {
                "History.TipLine5",
                new[]
                {
                    "메시지: {0}",
                    "Message: {0}",
                    "メッセージ: {0}"
                }
            },
            {
                "History.TipLine6",
                new[]
                {
                    "프리셋: {0}",
                    "Preset: {0}",
                    "プリセット: {0}"
                }
            },
            {
                "History.RowMenu.tip",
                new[]
                {
                    "이 기록에 대한 작업 메뉴를 엽니다. (행을 우클릭해도 같습니다)",
                    "Open the action menu for this record. (Right-clicking the row does the same)",
                    "この記録の操作メニューを開きます。（行の右クリックでも同じ）"
                }
            },
            {
                "History.OpenOutput",
                new[]
                {
                    "출력 폴더 열기",
                    "Open output folder",
                    "出力フォルダを開く"
                }
            },
            {
                "History.PingPrefab",
                new[]
                {
                    "소스 프리팹 보기",
                    "Show source prefab",
                    "ソースプレハブを表示"
                }
            },
            {
                "History.CopyPath",
                new[]
                {
                    "산출물 경로 복사",
                    "Copy output path",
                    "出力パスをコピー"
                }
            },
            {
                "History.DeleteOne",
                new[]
                {
                    "이 기록 삭제",
                    "Delete this record",
                    "この記録を削除"
                }
            },
            {
                "History.CopyDone",
                new[]
                {
                    "경로를 클립보드에 복사했습니다.\n{0}",
                    "Path copied to the clipboard.\n{0}",
                    "パスをクリップボードにコピーしました。\n{0}"
                }
            },
            {
                "History.NoOutputFile",
                new[]
                {
                    "산출 파일을 찾을 수 없습니다.\n{0}",
                    "Output file not found.\n{0}",
                    "出力ファイルが見つかりません。\n{0}"
                }
            },
            {
                "History.NoPrefab",
                new[]
                {
                    "소스 프리팹을 찾을 수 없습니다. 프로젝트에서 이동되었거나 삭제되었을 수 있습니다.\n{0}",
                    "Source prefab not found. It may have been moved or deleted.\n{0}",
                    "ソースプレハブが見つかりません。移動または削除された可能性があります。\n{0}"
                }
            },
            {
                "History.FixupBadge",
                new[]
                {
                    "자동 정리",
                    "Tidied",
                    "自動整理"
                }
            },
            {
                "History.ShowFixups",
                new[]
                {
                    "자동 정리 내역 보기",
                    "Show what was tidied",
                    "自動整理の内容を見る"
                }
            },
            {
                "History.ShowFixups.tip",
                new[]
                {
                    "이 빌드에서 빌드용 복사본을 자동으로 정리한 내역을 창으로 보여줍니다.",
                    "Show, in a dialog, what was tidied automatically in the build copy for this build.",
                    "このビルドでビルド用コピーを自動整理した内容をダイアログで表示します。"
                }
            },
            {
                "History.RecoveryDismiss",
                new[]
                {
                    "알림 닫기",
                    "Dismiss",
                    "閉じる"
                }
            },
            {
                "History.RecoveryDismiss.tip",
                new[]
                {
                    "이번 에디터 세션 동안 이 알림을 숨깁니다. 정리 결과는 콘솔 로그에도 남아 있고, 아무것도 지우지 않습니다.",
                    "Hide this notice for the rest of this editor session. The result is also in the Console log; nothing is deleted.",
                    "このエディターセッション中はこの通知を隠します。結果はコンソールログにも残っており、何も削除しません。"
                }
            },
            {
                "History.StrandedList",
                new[]
                {
                    "남은 프리팹 목록 ({0}개)",
                    "Prefabs left behind ({0})",
                    "残ったプレハブ一覧 ({0} 個)"
                }
            },
            {
                "History.StrandedList.tip",
                new[]
                {
                    "항목마다 [프로젝트에서 보기] 로 위치를 확인한 뒤 원래 폴더로 직접 옮기세요. 옮긴 뒤 [새로고침] 을 누르면 이 안내가 갱신됩니다.",
                    "Use [Show in Project] on each item to find it, then move it back to its original folder yourself. Press [Refresh] afterwards to update this notice.",
                    "各項目の [プロジェクトで表示] で場所を確認し、元のフォルダへ手動で戻してください。戻した後 [更新] を押すとこの案内が更新されます。"
                }
            },
            {
                "History.StrandedItemTip",
                new[]
                {
                    "모드: {0}\n원래 위치: {1}",
                    "Mod: {0}\nOriginal location: {1}",
                    "MOD: {0}\n元の場所: {1}"
                }
            },
            {
                "History.StrandedDismiss",
                new[]
                {
                    "알림 닫기",
                    "Dismiss",
                    "閉じる"
                }
            },
            {
                "History.StrandedDismiss.tip",
                new[]
                {
                    "지금 목록의 프리팹에 대한 이 알림을 이 프로젝트에서 숨깁니다. 나중에 목록에 없던 프리팹이 새로 생기면 다시 보입니다. 파일은 옮기지 않습니다.",
                    "Hide this notice in this project for the prefabs listed now. It shows again if a prefab that is not on this list turns up later. No file is moved.",
                    "現在の一覧のプレハブについて、このプロジェクトではこの通知を隠します。後で一覧にないプレハブが新たに見つかると再び表示されます。ファイルは移動しません。"
                }
            },
            {
                "History.RebuildSelectedNone.tip",
                new[]
                {
                    "선택한 기록이 없어서 꺼져 있습니다. 목록에서 기록을 체크하면 켜집니다.",
                    "Off because no record is selected. Check a record in the list to turn it on.",
                    "選択した記録がないためオフです。一覧で記録にチェックするとオンになります。"
                }
            },
            {
                "History.RebuildFailedNone.tip",
                new[]
                {
                    "지금 목록에 실패한 기록이 없어서 꺼져 있습니다. 실패한 기록이 목록에 보이면 켜집니다(상태 필터의 '실패'가 켜져 있어야 합니다).",
                    "Off because the current list has no failed record. It turns on when a failed record is listed (the 'Failed' status filter must be on).",
                    "現在の一覧に失敗した記録がないためオフです。失敗した記録が一覧に表示されるとオンになります(状態フィルターの「失敗」がオンである必要があります)。"
                }
            },
            {
                "History.DeleteSelectedNone.tip",
                new[]
                {
                    "선택한 기록이 없어서 꺼져 있습니다. 목록에서 지울 기록을 체크하면 켜집니다.",
                    "Off because no record is selected. Check the records to delete in the list to turn it on.",
                    "選択した記録がないためオフです。一覧で削除する記録にチェックするとオンになります。"
                }
            },
            {
                "History.CleanOldNone.tip",
                new[]
                {
                    "{0}일보다 오래된 기록이 없어서 꺼져 있습니다. 시작한 지 {0}일이 지난 기록이 생기면 켜집니다.",
                    "Off because no record is older than {0} days. It turns on once a record started more than {0} days ago.",
                    "{0}日より古い記録がないためオフです。開始から{0}日を過ぎた記録ができるとオンになります。"
                }
            },
            {
                "History.ClearAllNone.tip",
                new[]
                {
                    "지울 기록이 없어서 꺼져 있습니다. 빌드하면 기록이 쌓이고 켜집니다.",
                    "Off because there is no record to delete. It turns on once a build is recorded.",
                    "削除する記録がないためオフです。ビルドすると記録がたまり、オンになります。"
                }
            },
            {
                "History.RebuildSend",
                new[]
                {
                    "빌드 탭으로 보내기",
                    "Send to Build tab",
                    "ビルドタブに送る"
                }
            },
            {
                "History.StatusClose.tip",
                new[]
                {
                    "이 알림을 닫습니다.",
                    "Close this message.",
                    "このお知らせを閉じます。"
                }
            },
            {
                "History.RebuildSelected",
                new[]
                {
                    "선택 재빌드",
                    "Rebuild selected",
                    "選択を再ビルド"
                }
            },
            {
                "History.RebuildSelected.tip",
                new[]
                {
                    "선택한 기록의 프리팹을 다시 빌드합니다. 같은 프리팹은 가장 최근 기록 1건으로 합쳐집니다.",
                    "Rebuild the prefabs of the selected records. Duplicates of the same prefab collapse into the newest one.",
                    "選択した記録のプレハブを再ビルドします。同じプレハブは最新の1件にまとめられます。"
                }
            },
            {
                "History.RebuildFailedOnly",
                new[]
                {
                    "실패만 재빌드",
                    "Rebuild failed only",
                    "失敗のみ再ビルド"
                }
            },
            {
                "History.RebuildFailedOnly.tip",
                new[]
                {
                    "지금 목록에 보이는 실패 기록만 모읍니다. 같은 프리팹은 가장 최근 실패 1건만 남깁니다.",
                    "Collect only the failed records currently listed, keeping the newest failure per prefab.",
                    "現在表示中の失敗記録だけを集め、同じプレハブは最新の失敗1件のみ残します。"
                }
            },
            {
                "History.RebuildConfirm",
                new[]
                {
                    "'{0}' 을(를) 빌드 탭으로 보냅니다.\n소스 프리팹: {1}\n\n계속할까요?",
                    "Send '{0}' to the Build tab.\nSource prefab: {1}\n\nContinue?",
                    "「{0}」をビルドタブに送ります。\nソースプレハブ: {1}\n\n続行しますか？"
                }
            },
            {
                "History.RebuildFailedConfirm",
                new[]
                {
                    "실패 기록에서 재빌드 대상 {0}건을 찾았습니다. (같은 프리팹은 최신 1건으로 합침)\n첫 대상: {1}\n\n계속할까요?",
                    "Found {0} rebuild target(s) among failed records (newest per prefab).\nFirst target: {1}\n\nContinue?",
                    "失敗記録から再ビルド対象 {0}件を見つけました。（同じプレハブは最新1件）\n最初の対象: {1}\n\n続行しますか？"
                }
            },
            {
                "History.RebuildMultiNotice",
                new[]
                {
                    "{0}건을 빌드 탭 선택에 넣었습니다 (예: '{1}').\n\n" + "선택한 기록의 타입 · 버전 · 제작자 · 출력 폴더가 서로 달라서, 한 번에 빌드할 수 있는 가장 큰 묶음만 넘겼습니다. " + "이 묶음을 빌드한 뒤 남은 {2}건을 골라 다시 [선택 재빌드] 또는 [실패만 재빌드] 를 눌러 주세요.",
                    "Queued {0} item(s) in the Build tab (e.g. '{1}').\n\n" + "The selected records use different types, versions, authors or output folders, so only the largest group that can be built together was sent. " + "After this build, select the remaining {2} and press [Rebuild selected] or [Rebuild failed only] again.",
                    "{0}件をビルドタブの選択に入れました（例: 「{1}」）。\n\n" + "選択した記録のタイプ・バージョン・作者・出力フォルダが異なるため、一度にビルドできる最も大きなまとまりだけを送りました。" + "このビルドのあと、残りの {2}件を選んで [選択を再ビルド] または [失敗のみ再ビルド] を再度押してください。"
                }
            },
            {
                "History.DeleteSelected",
                new[]
                {
                    "선택 삭제",
                    "Delete selected",
                    "選択を削除"
                }
            },
            {
                "History.DeleteSelected.tip",
                new[]
                {
                    "선택한 기록만 이력에서 지웁니다. 산출물(.warudo) 파일은 지워지지 않습니다.",
                    "Remove the selected records from the log only. The .warudo output files are not touched.",
                    "選択した記録だけを履歴から削除します。出力(.warudo)ファイルは削除されません。"
                }
            },
            {
                "History.DeleteSelectedConfirm",
                new[]
                {
                    "선택한 기록 {0}건을 이력에서 삭제합니다.\n(첫 항목: {1})\n\n산출물 파일은 지워지지 않고 기록만 사라집니다. 되돌릴 수 없습니다.",
                    "Delete {0} selected record(s) from the history.\n(first: {1})\n\nOutput files are kept. Only the log entries are removed. This cannot be undone.",
                    "選択した記録 {0}件を履歴から削除します。\n(最初: {1})\n\n出力ファイルは残り、記録のみ削除されます。元に戻せません。"
                }
            },
            {
                "History.DeleteOneConfirm",
                new[]
                {
                    "기록 1건 '{0}' ({1}) 을(를) 이력에서 삭제합니다.\n\n산출물 파일은 지워지지 않습니다. 되돌릴 수 없습니다.",
                    "Delete 1 record '{0}' ({1}) from the history.\n\nThe output file is kept. This cannot be undone.",
                    "記録1件「{0}」({1}) を履歴から削除します。\n\n出力ファイルは削除されません。元に戻せません。"
                }
            },
            {
                "History.DeleteDone",
                new[]
                {
                    "기록 {0}건을 삭제했습니다.",
                    "Deleted {0} record(s).",
                    "記録 {0}件を削除しました。"
                }
            },
            {
                "History.CleanOld",
                new[]
                {
                    "90일 이전 기록 정리",
                    "Clean up 90+ day old records",
                    "90日より前の記録を整理"
                }
            },
            {
                "History.CleanOld.tip",
                new[]
                {
                    "시작 시각이 90일보다 오래된 기록을 지웁니다. 산출물 파일은 건드리지 않습니다.",
                    "Remove records started more than 90 days ago. Output files are not touched.",
                    "開始時刻が90日より古い記録を削除します。出力ファイルには触れません。"
                }
            },
            {
                "History.CleanOldConfirm",
                new[]
                {
                    "{0}일보다 오래된 기록 {1}건을 삭제합니다.\n(가장 오래된 항목: {2})\n\n산출물 파일은 그대로 남습니다. 되돌릴 수 없습니다.",
                    "Delete {1} record(s) older than {0} days.\n(oldest: {2})\n\nOutput files are kept. This cannot be undone.",
                    "{0}日より古い記録 {1}件を削除します。\n(最も古い項目: {2})\n\n出力ファイルは残ります。元に戻せません。"
                }
            },
            {
                "History.CleanOldRun",
                new[]
                {
                    "정리",
                    "Clean up",
                    "整理する"
                }
            },
            {
                "History.ClearAll",
                new[]
                {
                    "전체 삭제",
                    "Delete all",
                    "全削除"
                }
            },
            {
                "History.ClearAll.tip",
                new[]
                {
                    "빌드 이력 전체를 지웁니다. 산출물 파일은 지워지지 않지만 어떤 산출물이 어느 프리팹에서 나왔는지 추적할 수 없게 됩니다.",
                    "Wipe the whole build history. Output files stay, but you lose the link between outputs and source prefabs.",
                    "ビルド履歴を全て削除します。出力ファイルは残りますが、出力とソースプレハブの対応が追えなくなります。"
                }
            },
            {
                "History.ClearAllConfirm1",
                new[]
                {
                    "빌드 이력 전체 {0}건을 삭제합니다.\n\n산출물 파일은 남지만, 어떤 .warudo 가 어느 프리팹에서 나왔는지 추적할 수 없게 됩니다.",
                    "Delete all {0} history records.\n\nOutput files remain, but you will no longer know which .warudo came from which prefab.",
                    "ビルド履歴 全 {0}件を削除します。\n\n出力ファイルは残りますが、どの .warudo がどのプレハブ由来か分からなくなります。"
                }
            },
            {
                "History.ClearAllConfirm2",
                new[]
                {
                    "정말로 {0}건을 전부 지울까요?\n되돌릴 수 없습니다. 필요하면 먼저 [CSV 내보내기] 로 백업하세요.",
                    "Really delete all {0} records?\nThis cannot be undone. Back up with [Export CSV] first if needed.",
                    "本当に {0}件を全て削除しますか？\n元に戻せません。必要なら先に [CSVエクスポート] でバックアップしてください。"
                }
            },
            {
                "History.ExportCsv",
                new[]
                {
                    "CSV 내보내기…",
                    "Export CSV…",
                    "CSVエクスポート…"
                }
            },
            {
                "History.ExportCsv.tip",
                new[]
                {
                    "이력 전체(필터와 무관)를 CSV 파일로 저장합니다. 엑셀에서 바로 열 수 있습니다.",
                    "Save the whole history (ignoring filters) as a CSV file, ready for Excel.",
                    "履歴全体(フィルター無関係)をCSVファイルに保存します。Excelでそのまま開けます。"
                }
            },
            {
                "History.ExportTitle",
                new[]
                {
                    "빌드 이력을 CSV 로 저장",
                    "Save build history as CSV",
                    "ビルド履歴をCSVで保存"
                }
            },
            {
                "History.ExportDone",
                new[]
                {
                    "이력 {0}건을 CSV 로 저장했습니다.\n{1}",
                    "Exported {0} record(s) to CSV.\n{1}",
                    "履歴 {0}件をCSVに保存しました。\n{1}"
                }
            },
            {
                "History.ExportFailed",
                new[]
                {
                    "CSV 저장에 실패했습니다.\n{0}",
                    "Failed to export CSV.\n{0}",
                    "CSVの保存に失敗しました。\n{0}"
                }
            },
            {
                "History.SearchHint",
                new[]
                {
                    "이름으로 걸러냅니다. 접두사도 쓸 수 있습니다:\n" + "type:prop · ver:1.0 · author:이름 · job:프리셋 · state:success|fail|other · file:yes|no\n" + "여러 단어를 공백으로 넣으면 모두 만족하는 기록만 남습니다.",
                    "Filter by name. Prefixes are supported:\n" + "type:prop · ver:1.0 · author:name · job:preset · state:success|fail|other · file:yes|no\n" + "Space-separated terms must all match.",
                    "名前で絞り込みます。接頭辞も使えます:\n" + "type:prop · ver:1.0 · author:名前 · job:プリセット · state:success|fail|other · file:yes|no\n" + "スペース区切りの語は全て満たす記録のみ残ります。"
                }
            },
            {
                "History.TypeShortProp",
                new[]
                {
                    "소품",
                    "Prop",
                    "小物"
                }
            },
            {
                "History.TypeShortCharacter",
                new[]
                {
                    "캐릭터",
                    "Char",
                    "キャラ"
                }
            },
            {
                "History.TypeShortEnvironment",
                new[]
                {
                    "환경",
                    "Env",
                    "環境"
                }
            },
            {
                "History.RebuildSelectedConfirm",
                new[]
                {
                    "선택한 기록에서 재빌드 대상 {0}건을 찾았습니다. (같은 프리팹은 최신 1건으로 합침)\n첫 대상: {1}\n\n계속할까요?",
                    "Found {0} rebuild target(s) in the selection (newest per prefab).\nFirst target: {1}\n\nContinue?",
                    "選択した記録から再ビルド対象 {0}件を見つけました。（同じプレハブは最新1件）\n最初の対象: {1}\n\n続行しますか？"
                }
            },
        };
    }
}
