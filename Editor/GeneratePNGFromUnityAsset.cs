// ===================================
//
// Author  : copocopo
// License : licensed under the CC0 1.0 Universal
// https://github.com/coposuke/UnityAssetToPNG
//
// ===================================


using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using System;
using System.IO;
using System.Linq;


namespace CopoCopo
{
	/// <summary>
	/// PNG 生成クラス
	/// </summary>
	public class GeneratePNG
	{
		/// <summary>
		/// メッセージ
		/// </summary>
		private enum Message
		{
			[EnumOption("")]
			Success,
			[EnumOption("Warning : サポートしていないアセットタイプです")]
			UnsupportedError,
			[EnumOption("Warning : ゲームを実行中でないと正しい変換が出来ません")]
			NonPlayingError,
			[EnumOption("Warning : 変換するアセットを選択してください")]
			UnselectedError,
			[EnumOption("Warning : 選択したアトラスにスプライトが1つもありません")]
			SpriteLengthError,
			[EnumOption("FatalError : アセットのロードあるいはキャストに失敗しました")]
			LoadError,
			[EnumOption("FatalError : 致命的なエラーが発生しました")]
			FatalError,
		}

		/// <summary>
		/// 変換結果
		/// </summary>
		private struct Result
		{
			public Texture2D texture;
			public Message msg;

			static public Result Notify(Message msg)
			{
				return new Result() {
					msg = msg,
					texture = null,
				};
			}

			static public Result Notify(Texture2D texture)
			{
				return new Result()
				{
					msg = (texture != null) ? Message.Success : Message.FatalError,
					texture = texture,
				};
			}
		}


		/// <summary>
		/// 選択したアセットから PNG を生成する
		/// </summary>
		/// <note>選択したSpriteAtlasと同階層にPackされたPNGが出力される</note>
		[MenuItem("Assets/Generate PNG")]
		static void Execute()
		{
			int instanceID = Selection.activeInstanceID;
			string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

			if (string.IsNullOrWhiteSpace(path))
			{
				Debug.LogWarning(Message.UnselectedError.ToOption());
				return;
			}

			Result result = default;
			if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) != null)
				result = GenerateFromSpriteAtlas(path);
			else if (AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray().Length > 0)
				result = GenerateFromSprites(path);
			else if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null)
				result = GenerateFromTexture2D(path);
			else
				result = Result.Notify(Message.UnsupportedError);

			if (result.msg != Message.Success)
			{
				Debug.LogWarning(result.msg.ToOption());
				return;
			}

			var texture = result.texture;

			string outputPath = Directory.GetParent(path).FullName;
			outputPath = Path.Combine(outputPath, texture.name);
			outputPath = Path.ChangeExtension(outputPath, "png");

			for (int i = 0; File.Exists(outputPath); ++i)
			{
				outputPath = Directory.GetParent(outputPath).FullName;
				outputPath = Path.Combine(outputPath, texture.name + i);
				outputPath = Path.ChangeExtension(outputPath, "png");
			}

			File.WriteAllBytes(outputPath, texture.EncodeToPNG());
			AssetDatabase.Refresh();

			Debug.Log(
				"Finish Output!!\n" +
				"source: " + texture + "\n" +
				"output: " + outputPath);
		}

		/// <summary>
		/// SpriteAtlas から Texture2D を生成する
		/// </summary>
		static Result GenerateFromSpriteAtlas(string path)
		{
			if (!Application.isPlaying)
				return Result.Notify(Message.NonPlayingError);

			var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
			if (spriteAtlas == null)
				return Result.Notify(Message.LoadError);

			Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
			spriteAtlas.GetSprites(sprites);

			if (sprites.Length <= 0)
				return Result.Notify(Message.SpriteLengthError);

			return Result.Notify(ToReadableTexture2D(sprites[0].texture));
		}

		/// <summary>
		/// Texture2D をロードする
		/// </summary>
		static Result GenerateFromTexture2D(string path)
		{
			var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			if (texture == null)
				return Result.Notify(Message.LoadError);

			return Result.Notify(ToReadableTexture2D(texture));
		}

		/// <summary>
		/// Sprite から PNG を生成する
		/// </summary>
		static Result GenerateFromSprites(string path)
		{
			var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
			if (sprites == null)
				return Result.Notify(Message.LoadError);
			if (sprites.Length <= 0)
				return Result.Notify(Message.SpriteLengthError);

			int widthPerOne = Mathf.CeilToInt(sprites.Max(s => s.rect.width));
			int heightPerOne = Mathf.CeilToInt(sprites.Max(s => s.rect.height));
			int row = 0;

			for (int i = 1; i < 30; i++)
			{
				if (sprites.Length < i * i)
				{
					row = i;
					break;
				}
			}

			var atlas = ToReadableTexture2D(sprites[0].texture);
			var texture = new Texture2D(
				Mathf.CeilToInt(row * widthPerOne),
				Mathf.CeilToInt(row * heightPerOne),
				TextureFormat.RGBA32, false);
			texture.name = atlas.name;

			for (int x = 0; x < texture.width; ++x)
				for (int y = 0; y < texture.height; ++y)
					texture.SetPixel(x, y, Color.clear);

			for (int rowY = 0; rowY < row; ++rowY)
			{
				for (int rowX = 0; rowX < row; ++rowX)
				{
					int i = rowY * row + rowX;

					if (sprites.Length <= i)
						break;

					var sprite = sprites[i];
					int spriteWidth = Mathf.FloorToInt(sprite.rect.width);
					int spriteHeight = Mathf.FloorToInt(sprite.rect.height);
					var atlasRect = sprite.rect;
					int startX = (widthPerOne - spriteWidth) / 2;
					int startY = (heightPerOne - spriteHeight) / 2;
					int textureStartX = rowX * widthPerOne;
					int textureStartY = texture.height - rowY * heightPerOne;

					for (int x = 0; x < spriteWidth; ++x)
					{
						for (int y = 0; y < spriteHeight; ++y)
						{
							var color = atlas.GetPixel(Mathf.FloorToInt(atlasRect.x + x), Mathf.FloorToInt(atlasRect.y + spriteHeight - y));
							texture.SetPixel(textureStartX + startX + x, textureStartY - startY - y, color);
						}
					}
				}
			}

			return Result.Notify(texture);
		}

		/// <summary>
		/// 読み取り不可のアセット回避
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		static Texture2D ToReadableTexture2D(Texture2D src)
		{
			RenderTexture dst = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(src, dst);

			RenderTexture temp = RenderTexture.active;
			RenderTexture.active = dst;

			var output = new Texture2D(dst.width, dst.height, TextureFormat.ARGB32, false);
			output.name = src.name;
			output.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0);
			output.Apply(false);

			RenderTexture.active = temp;
			RenderTexture.ReleaseTemporary(dst);

			return output;
		}
	}

	/// <summary>
	/// Enumに文字情報を追加するための属性
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class EnumOptionAttribute : Attribute
	{
		public EnumOptionAttribute(string value)
		{
			this.option = value;
		}

		public string option { get; private set; }
	}

	/// <summary>
	/// Enumに設定された文字情報を取り出すメソッド
	/// </summary>
	public static class EnumExtention
	{
		public static string ToOption(this Enum enm)
		{
			return enm.GetType()
				.GetField(enm.ToString())
				.GetCustomAttributes(typeof(EnumOptionAttribute), false)
				.Cast<EnumOptionAttribute>()
				.Single()
				.option;
		}
	}
}
