# UnityAddressableAutoAddress

指定したパス内にあるアセットに自動的に address を付与するツールです。

<img src="https://github.com/yayorozu/ImageUploader/blob/master/AutoAddress/Top.png" width="500" alt="UnityAddressableAutoAddress Top">

---

## 使い方

1. **ウィンドウの起動**  
   Unity エディタのメニューから `Tools/AddressableAutoAddress` を選択してウィンドウを開きます。

2. **フォルダの設定**  
   - **ObserveTargetFolder:**  
     対象のディレクトリを設定します。  
     設定したディレクトリの子ディレクトリが Addressable Group として自動的に設定されます。  
     
     例えば、以下のようなフォルダ構成の場合、`Local` と `Remote` のグループが作成されます。
     
     ```
     AddressableResources
     ┿ Local
     ┗ Remote
     ```
     
     このフォルダ以下に存在するアセットには自動的に address が付与されます。

     <img src="https://github.com/yayorozu/ImageUploader/blob/master/AutoAddress/PathSample.png" width="500" alt="UnityAddressableAutoAddress Top">

3. **パススクリプトの自動生成**  
   - **GeneratePathScript:**  
     チェックを入れておくと、対象のディレクトリ内にパス用のスクリプトが自動生成されます。  
     
     生成されるスクリプトは、ロード時に address を直接指定する手間を省くために利用できます。  
     例えば、以下のようなスクリプトが生成されます:
     
     ```csharp
     public static class AutoAddressPath
     {
         // Assets/AddressableResources/Local/Prefab/UI/Study/UIStudyQuestion.prefab
         public static string PrefabUIStudy_UIStudyQuestion => "Prefab/UI/Study/UIStudyQuestion";
         public static AsyncOperationHandle<UnityEngine.GameObject> PrefabUIStudy_UIStudyQuestionHandle =>
             Addressables.LoadAssetAsync<UnityEngine.GameObject>(PrefabUIStudy_UIStudyQuestion);
         // ・・・
     }
     ```
     
     このスクリプトを利用することで、コード内でアセットのパス指定を簡略化できます。


# ライセンス

本プロジェクトは [MIT License](LICENSE) の下でライセンスされています。  
詳細については、LICENSE ファイルをご覧ください。
