﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Souvenir;
using UnityEngine;

public partial class SouvenirModule
{
    private IEnumerable<object> ProcessDACHMaze(KMBombModule module)
    {
        return processWorldMaze(module, "DACHMaze", _DACHMaze, Question.DACHMazeOrigin);
    }

    private IEnumerable<object> ProcessDeafAlley(KMBombModule module)
    {
        var comp = GetComponent(module, "DeafAlleyScript");
        var fldSolved = GetField<bool>(comp, "moduleSolved");
        var shapes = GetField<string[]>(comp, "shapes").Get();

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DeafAlley);

        var selectedShape = GetField<int>(comp, "selectedShape").Get();
        addQuestions(module, makeQuestion(Question.DeafAlleyShape, _DeafAlley, correctAnswers: new[] { shapes[selectedShape] }, preferredWrongAnswers: shapes));
    }

    private IEnumerable<object> ProcessDeckOfManyThings(KMBombModule module)
    {
        var comp = GetComponent(module, "deckOfManyThingsScript");
        var fldSolved = GetField<bool>(comp, "moduleSolved");
        var fldSolution = GetIntField(comp, "solution");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DeckOfManyThings);

        var deck = GetField<Array>(comp, "deck").Get(d => d.Length == 0 ? "deck is empty" : null);
        var btns = GetArrayField<KMSelectable>(comp, "btns", isPublic: true).Get(expectedLength: 2);
        var prevCard = GetField<KMSelectable>(comp, "prevCard", isPublic: true).Get();
        var nextCard = GetField<KMSelectable>(comp, "nextCard", isPublic: true).Get();

        prevCard.OnInteract = delegate { return false; };
        nextCard.OnInteract = delegate { return false; };
        foreach (var btn in btns)
            btn.OnInteract = delegate
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
                btn.AddInteractionPunch(0.5f);
                return false;
            };

        string firstCardDeck = deck.GetValue(0).GetType().ToString().Replace("Card", "");

        // correcting original misspelling
        if (firstCardDeck == "Artic")
            firstCardDeck = "Arctic";

        var solution = fldSolution.Get();

        if (solution == 0)
        {
            Debug.LogFormat("[Souvenir #{0}] No question for The Deck of Many Things because the solution was the first card.", _moduleId);
            _legitimatelyNoQuestions.Add(module);
            yield break;
        }

        addQuestion(module, Question.DeckOfManyThingsFirstCard, correctAnswers: new[] { firstCardDeck });
    }

    private IEnumerable<object> ProcessDecoloredSquares(KMBombModule module)
    {
        var comp = GetComponent(module, "DecoloredSquaresModule");
        var fldSolved = GetField<bool>(comp, "_isSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DecoloredSquares);

        var colColor = GetField<string>(comp, "_color1").Get();
        var rowColor = GetField<string>(comp, "_color2").Get();

        addQuestions(module,
            makeQuestion(Question.DecoloredSquaresStartingPos, _DecoloredSquares, formatArgs: new[] { "column" }, correctAnswers: new[] { colColor }),
            makeQuestion(Question.DecoloredSquaresStartingPos, _DecoloredSquares, formatArgs: new[] { "row" }, correctAnswers: new[] { rowColor }));
    }

    private IEnumerable<object> ProcessDevilishEggs(KMBombModule module)
    {
        var comp = GetComponent(module, "devilishEggs");
        var fldSolved = GetField<bool>(comp, "moduleSolved");
        var prismTexts = GetArrayField<TextMesh>(comp, "prismTexts", isPublic: true).Get(expectedLength: 3);
        var digits = prismTexts[0].text.Split(' ');
        var letters = prismTexts[1].text.Split(' ');
        if (digits.Length != 8 || digits.Any(str => str.Length != 1 || str[0] < '0' || str[0] > '9'))
            throw new AbandonModuleException("Expected 8 digits; got {0} ({1})", digits.Length, digits.JoinString("; "));
        if (letters.Length != 8 || letters.Any(str => str.Length != 1 || str[0] < 'A' || str[0] > 'Z'))
            throw new AbandonModuleException("Expected 8 letters; got {0} ({1})", letters.Length, letters.JoinString("; "));

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DevilishEggs);

        var topRotations = GetField<Array>(comp, "topRotations").Get(validator: arr => arr.Length != 6 ? "expected length 6" : null).Cast<object>().Select(rot => rot.ToString()).ToArray();
        var bottomRotations = GetField<Array>(comp, "bottomRotations").Get(validator: arr => arr.Length != 6 ? "expected length 6" : null).Cast<object>().Select(rot => rot.ToString()).ToArray();
        var allRotations = topRotations.Concat(bottomRotations).ToArray();

        var qs = new List<QandA>();
        for (var rotIx = 0; rotIx < 6; rotIx++)
        {
            qs.Add(makeQuestion(Question.DevilishEggsRotations, _DevilishEggs, formatArgs: new[] { "top", ordinal(rotIx + 1) }, correctAnswers: new[] { topRotations.GetValue(rotIx).ToString() }, preferredWrongAnswers: allRotations));
            qs.Add(makeQuestion(Question.DevilishEggsRotations, _DevilishEggs, formatArgs: new[] { "bottom", ordinal(rotIx + 1) }, correctAnswers: new[] { bottomRotations.GetValue(rotIx).ToString() }, preferredWrongAnswers: allRotations));
        }
        for (var ix = 0; ix < 8; ix++)
        {
            qs.Add(makeQuestion(Question.DevilishEggsNumbers, _DevilishEggs, formatArgs: new[] { ordinal(ix + 1) }, correctAnswers: new[] { digits[ix] }, preferredWrongAnswers: digits));
            qs.Add(makeQuestion(Question.DevilishEggsLetters, _DevilishEggs, formatArgs: new[] { ordinal(ix + 1) }, correctAnswers: new[] { letters[ix] }, preferredWrongAnswers: letters));
        }
        addQuestions(module, qs);
    }

    private IEnumerable<object> ProcessDiscoloredSquares(KMBombModule module)
    {
        var comp = GetComponent(module, "DiscoloredSquaresModule");
        var fldSolved = GetField<bool>(comp, "_isSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DiscoloredSquares);

        var colorsRaw = GetField<Array>(comp, "_rememberedColors").Get(arr => arr.Length != 4 ? "expected length 4" : null);
        var positions = GetArrayField<int>(comp, "_rememberedPositions").Get(expectedLength: 4);
        var colors = colorsRaw.Cast<object>().Select(obj => obj.ToString()).ToArray();

        addQuestions(module, Enumerable.Range(0, 4).Select(color =>
            makeQuestion(Question.DiscoloredSquaresRememberedPositions, _DiscoloredSquares, formatArgs: new[] { colors[color] }, correctAnswers: new[] { new Coord(4, 4, positions[color]) })));
    }

    private IEnumerable<object> ProcessDivisibleNumbers(KMBombModule module)
    {
        var comp = GetComponent(module, "DivisableNumbers");
        var fldSolved = GetField<bool>(comp, "moduleSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DivisibleNumbers);

        var finalAnswers = GetArrayField<string>(comp, "finalAnswers").Get(expectedLength: 3, validator: answer => answer != "Yea" && answer != "Nay" ? "expected either “Yea” or “Nea”" : null);
        var finalNumbers = GetArrayField<int>(comp, "finalNumbers").Get(expectedLength: 3, validator: number => number < 0 || number > 9999 ? "expected range 0–9999" : null);
        var finalNumbersStr = finalNumbers.Select(n => n.ToString()).ToArray();

        var qs = new List<QandA>();
        for (int i = 0; i < finalNumbers.Length; i++)
            qs.Add(makeQuestion(Question.DivisibleNumbersNumbers, _DivisibleNumbers, formatArgs: new[] { ordinal(i + 1) }, correctAnswers: new[] { finalNumbersStr[i] }, preferredWrongAnswers: finalNumbersStr));
        qs.Add(makeQuestion(Question.DivisibleNumbersAnswers, _DivisibleNumbers, correctAnswers: new[] { string.Join(", ", finalAnswers) }));
        addQuestions(module, qs);
    }

    private IEnumerable<object> ProcessDoubleColor(KMBombModule module)
    {
        var comp = GetComponent(module, "doubleColor");
        var fldSolved = GetField<bool>(comp, "_isSolved");
        var fldColor = GetIntField(comp, "screenColor");
        var fldStage = GetIntField(comp, "stageNumber");

        while (!_isActivated)
            yield return new WaitForSeconds(.1f);

        var color1 = fldColor.Get(min: 0, max: 4);
        var stage = fldStage.Get(min: 1, max: 1);
        var submitBtn = GetField<KMSelectable>(comp, "submit", isPublic: true).Get();

        var prevInteract = submitBtn.OnInteract;
        submitBtn.OnInteract = delegate
        {
            var ret = prevInteract();
            stage = fldStage.Get();
            if (stage == 1)  // This means the user got a strike. Need to retrieve the new first stage color
                // We mustn’t throw an exception inside of the button handler, so don’t check min/max values here
                color1 = fldColor.Get();
            return ret;
        };

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DoubleColor);

        // Check the value of color1 because we might have reassigned it inside the button handler
        if (color1 < 0 || color1 > 4)
            throw new AbandonModuleException(@"First stage color has unexpected value: {0} (expected 0 to 4).", color1);

        var color2 = fldColor.Get(min: 0, max: 4);

        var colorNames = new[] { "Green", "Blue", "Red", "Pink", "Yellow" };

        addQuestions(module,
            makeQuestion(Question.DoubleColorColors, _DoubleColor, formatArgs: new[] { "first" }, correctAnswers: new[] { colorNames[color1] }),
            makeQuestion(Question.DoubleColorColors, _DoubleColor, formatArgs: new[] { "second" }, correctAnswers: new[] { colorNames[color2] }));
    }

    private IEnumerable<object> ProcessDoubleOh(KMBombModule module)
    {
        var comp = GetComponent(module, "DoubleOhModule");
        var fldSolved = GetField<bool>(comp, "_isSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DoubleOh);

        var submitIndex = GetField<Array>(comp, "_functions").Get().Cast<object>().IndexOf(f => f.ToString() == "Submit");
        if (submitIndex < 0 || submitIndex > 4)
            throw new AbandonModuleException(@"Submit button is at index {0} (expected 0–4).", submitIndex);

        addQuestion(module, Question.DoubleOhSubmitButton, correctAnswers: new[] { "↕↔⇔⇕◆".Substring(submitIndex, 1) });
    }

    private IEnumerable<object> ProcessDrDoctor(KMBombModule module)
    {
        var comp = GetComponent(module, "DrDoctorModule");
        var fldSolved = GetField<bool>(comp, "_isSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DrDoctor);

        var diagnoses = GetArrayField<string>(comp, "_selectableDiagnoses").Get();
        var symptoms = GetArrayField<string>(comp, "_selectableSymptoms").Get();
        var diagnoseText = GetField<TextMesh>(comp, "DiagnoseText", isPublic: true).Get();

        addQuestions(module,
            makeQuestion(Question.DrDoctorDiseases, _DrDoctor, correctAnswers: diagnoses.Except(new[] { diagnoseText.text }).ToArray()),
            makeQuestion(Question.DrDoctorSymptoms, _DrDoctor, correctAnswers: symptoms));
    }

    private IEnumerable<object> ProcessDreamcipher(KMBombModule module)
    {
        var comp = GetComponent(module, "Dreamcipher");
        var fldSolved = GetField<bool>(comp, "moduleSolved");
        var wordList = JsonConvert.DeserializeObject<string[]>(GetField<TextAsset>(comp, "wordList", isPublic: true).Get().text);

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_Dreamcipher);

        string targetWord = GetField<string>(comp, "targetWord").Get().ToLowerInvariant();
        addQuestions(module, makeQuestion(Question.DreamcipherWord, _Dreamcipher, formatArgs: null, correctAnswers: new[] { targetWord }, preferredWrongAnswers: wordList));
    }

    private IEnumerable<object> ProcessDumbWaiters(KMBombModule module)
    {
        var comp = GetComponent(module, "dumbWaiters");
        var fldSolved = GetField<bool>(comp, "moduleSolved");

        while (!fldSolved.Get())
            yield return new WaitForSeconds(.1f);
        _modulesSolved.IncSafe(_DumbWaiters);

        var players = GetStaticField<string[]>(comp.GetType(), "names").Get();
        var playersAvaiable = GetArrayField<int>(comp, "presentPlayers").Get();
        var availablePlayers = playersAvaiable.Select(ix => players[ix]).ToArray();

        addQuestions(module,
           makeQuestion(Question.DumbWaitersPlayerAvailable, _DumbWaiters, formatArgs: new[] { "was" }, correctAnswers: availablePlayers, preferredWrongAnswers: players),
           makeQuestion(Question.DumbWaitersPlayerAvailable, _DumbWaiters, formatArgs: new[] { "was not" }, correctAnswers: players.Where(a => !availablePlayers.Contains(a)).ToArray(), preferredWrongAnswers: players));

    }
}