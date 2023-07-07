using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;

namespace UNIPlayer
{
    public static class UNIMLParser
    {
        public static (Vector2 Min, Vector2 Max) ParseAnchors(UNIAlign align)
        {
            switch (align)
            {
                case UNIAlign.lefttop:
                    return (new Vector2(0, 1), new Vector2(0, 1));
                case UNIAlign.top:
                    return (new Vector2(0.5f, 1), new Vector2(0.5f, 1));
                case UNIAlign.righttop:
                    return (new Vector2(1, 1), new Vector2(1, 1));
                case UNIAlign.right:
                    return (new Vector2(1, 0.5f), new Vector2(1, 0.5f));
                case UNIAlign.rightbottom:
                    return (new Vector2(1, 0), new Vector2(1, 0));
                case UNIAlign.bottom:
                    return (new Vector2(0.5f, 0), new Vector2(0.5f, 0));
                case UNIAlign.leftbottom:
                    return (new Vector2(0, 0), new Vector2(0, 0));
                case UNIAlign.left:
                    return (new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                case UNIAlign.center:
                    return (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                default:
                    return (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            }
        }

        public static Vector2 ParsePivot(UNIAlign pivot)
        {
            switch (pivot)
            {
                case UNIAlign.lefttop:
                    return new Vector2(0, 1);
                case UNIAlign.top:
                    return new Vector2(0.5f, 1);
                case UNIAlign.righttop:
                    return new Vector2(1, 1);
                case UNIAlign.right:
                    return new Vector2(1, 0.5f);
                case UNIAlign.rightbottom:
                    return new Vector2(1, 0);
                case UNIAlign.bottom:
                    return new Vector2(0.5f, 0);
                case UNIAlign.leftbottom:
                    return new Vector2(0, 0);
                case UNIAlign.left:
                    return new Vector2(0, 0.5f);
                case UNIAlign.center:
                    return new Vector2(0.5f, 0.5f);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        public static void AppendView(this RectTransform Container, UNIView view)
        {
            if (view.Buttons != null && view.Buttons.Count > 0)
            {
                view.Buttons.ForEach(_button =>
                {
                    Container
                        .AppendButton(_button)
                        .AddComponent<ObservablePointerDownTrigger>()
                        .OnPointerDownAsObservable()
                        .Subscribe(_ =>
                        {
                            Interpreter.ExecuteStatement(_button.onClick).Subscribe();
                        });
                });
            }
            if (view.Images != null && view.Images.Count > 0)
            {
                view.Images.ForEach(_image =>
                {
                    Container.AppendImage(_image);
                });
            }
        }

        public static Button AppendButton(this RectTransform Container, UNIButton btnNode)
        {
            return Container.AppendImage(btnNode).gameObject.AddComponent<Button>();
        }

        public static Image AppendImage(this RectTransform Container, UNIImage imgNode)
        {
            var _newImage = new GameObject();
            _newImage.transform.SetParent(Container);
            _newImage.transform.SetAsLastSibling();
            syncTransform(_newImage.transform.AddComponent<RectTransform>(), imgNode);
            _newImage.AddComponent<CanvasRenderer>();
            var _image = _newImage.AddComponent<Image>();
            if (imgNode.url != string.Empty)
            {
                Managements.Resource
                    .LoadTexture2D(Path.Combine(Application.streamingAssetsPath, imgNode.url))
                    .Subscribe(_tex =>
                    {
                        _image.sprite = Sprite.Create(
                            _tex,
                            new Rect(0, 0, _tex.width, _tex.height),
                            new Vector2(0, 0)
                        );
                        if (imgNode.match)
                        {
                            _image.SetNativeSize();
                        }
                    });
            }
            return _image;
        }

        private static void syncTransform(RectTransform rectTransform, UNITransform uniTransform)
        {
            rectTransform.localScale = Vector3.one * uniTransform.scale;

            rectTransform.pivot = UNIMLParser.ParsePivot(uniTransform.pivot);
            var _anchors = UNIMLParser.ParseAnchors(uniTransform.align);
            rectTransform.anchorMin = _anchors.Min;
            rectTransform.anchorMax = _anchors.Max;
            rectTransform.anchoredPosition = new Vector2(uniTransform.x, uniTransform.y);
            rectTransform.sizeDelta = new Vector2(uniTransform.width, uniTransform.height);
        }
    }
}
