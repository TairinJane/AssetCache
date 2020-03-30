﻿using System;
using System.Collections.Generic;

namespace AssetCache {
    public interface IAssetCache {
        /// <summary>
        /// Построение кэша для конкретного файла, так как файл потенциально может быть большим,
        /// необходимо проверять не попросили ли нас прерваться
        /// </summary>
        /// <param name="path">Путь к существующему файлу на диске</param>
        /// <param name="interruptChecker">проверка на необходимость прерваться, бросает 'OperationCanceledException' в случае необходимости прерваться</param>
        /// <returns>Данные кэша для данного файла</returns>
        object Build(string path, Action interruptChecker);

        /// <summary>
        /// Вызывается в случае успешного построения кэша для файла и складывает результат в общий индекс. Индекс используется для ответа на запросы, представленные ниже
        /// </summary>
        /// <param name="path">Путь к существующему файлу на диске</param>
        /// <param name="result">Данные кэша, полученный в функции 'Build,' для данного файла</param>
        void Merge(string path, object result);

        /// Следующая группа методов может вызываться только после построения кэша
        /// <summary>
        /// Внутри сцены у каждого объекта есть идентификатор,
        /// например 170076733 является идентификатором в заголовке "--- !u!1 &170076733",
        /// далее следует описание объекта. Данный идентификатор
        /// используется в описании объекта следующим образом: "... : {fileID: 170076733}"
        /// </summary>
        /// <param name="anchor">Идентификатор объекта</param>
        /// <returns>Сколько раз объект был использован в сцене</returns>
        int GetLocalAnchorUsages(ulong anchor);

        /// <summary>
        /// В Unity у каждого ресурса (файл с кодом, текстура, модель) существует .meta файл,
        /// в котором записывается guid данного объекта, ресурсы могут используются в сценах
        /// следующим образом: "... : {fileID: ..., guid: f70555f144d8491a825f0804e09c671c, type: ...}"
        /// (некоторые поля могут отсутствовать)
        /// </summary>
        /// <param name="guid">Идентификатор ресурса</param>
        /// <returns>Сколько раз ресурс используется в сцене</returns>
        int GetGuidUsages(string guid);

        /// <summary>
        /// Ключевое понятие в Unity - game object, к нему можно прикрепить различные компоненты,
        /// в том числе скрипты, написанные на c#. В файле сцены каждый game object в заголовке содержит "!u!1"
        /// Добавленные компоненты перечисляются в m_Component в описании объекта
        /// </summary>
        /// <param name="gameObjectAnchor">Идентификатор gameObject</param>
        /// <returns>Список идентификаторов компонент, которые прикреплены к объекту</returns>
        IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor);
    }
}