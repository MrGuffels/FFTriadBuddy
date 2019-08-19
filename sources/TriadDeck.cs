﻿using System;
using System.Collections.Generic;

namespace FFTriadBuddy
{
    public enum ETriadDeckState
    {
        Valid,
        MissingCards,
        HasDuplicates,
        TooManyRaresUncomon,
        TooManyRaresRare,
        TooManyRaresEpic,
    };

    public class TriadDeck
    {
        public List<TriadCard> knownCards;
        public List<TriadCard> unknownCardPool;

        public TriadDeck()
        {
            knownCards = new List<TriadCard>();
            unknownCardPool = new List<TriadCard>();
        }

        public TriadDeck(List<TriadCard> knownCards, List<TriadCard> unknownCardPool)
        {
            this.knownCards = new List<TriadCard>();
            this.unknownCardPool = new List<TriadCard>();

            this.knownCards.AddRange(knownCards);
            this.unknownCardPool.AddRange(unknownCardPool);
        }

        public TriadDeck(IEnumerable<TriadCard> knownCards)
        {
            this.knownCards = new List<TriadCard>();
            unknownCardPool = new List<TriadCard>();

            this.knownCards.AddRange(knownCards);
        }

        public TriadDeck(IEnumerable<int> knownCardIds, IEnumerable<int> unknownCardlIds)
        {
            TriadCardDB cardDB = TriadCardDB.Get();

            knownCards = new List<TriadCard>();
            foreach (int id in knownCardIds)
            {
                TriadCard card = cardDB.cards[id];
                if (card != null && card.IsValid())
                {
                    knownCards.Add(card);
                }
            }

            unknownCardPool = new List<TriadCard>();
            foreach (int id in unknownCardlIds)
            {
                TriadCard card = cardDB.cards[id];
                if (card != null && card.IsValid())
                {
                    unknownCardPool.Add(card);
                }
            }
        }

        public TriadDeck(IEnumerable<int> knownCardIds)
        {
            TriadCardDB cardDB = TriadCardDB.Get();

            knownCards = new List<TriadCard>();
            foreach (int id in knownCardIds)
            {
                TriadCard card = cardDB.cards[id];
                if (card != null && card.IsValid())
                {
                    knownCards.Add(card);
                }
            }

            unknownCardPool = new List<TriadCard>();
        }

        public ETriadDeckState GetDeckState()
        {
            PlayerSettingsDB playerDB = PlayerSettingsDB.Get();
            int[] rarityCounters = new int[5];

            for (int DeckIdx = 0; DeckIdx < knownCards.Count; DeckIdx++)
            {
                TriadCard deckCard = knownCards[DeckIdx];
                bool bIsOwned = playerDB.ownedCards.Contains(deckCard);
                if (!bIsOwned)
                {
                    return ETriadDeckState.MissingCards;
                }

                for (int TestIdx = 0; TestIdx < knownCards.Count; TestIdx++)
                {
                    if ((TestIdx != DeckIdx) && knownCards[TestIdx].Equals(deckCard))
                    {
                        return ETriadDeckState.HasDuplicates;
                    }
                }

                rarityCounters[(int)deckCard.Rarity]++;
            }

            int numRare45 = rarityCounters[(int)ETriadCardRarity.Epic] + rarityCounters[(int)ETriadCardRarity.Legendary];
            int numRare345 = rarityCounters[(int)ETriadCardRarity.Rare] + numRare45;
            int numRare2345 = rarityCounters[(int)ETriadCardRarity.Uncommon] + numRare345;

            if (playerDB.ownedCards.Count < 30)
            {
                if (numRare2345 > 1)
                {
                    return ETriadDeckState.TooManyRaresUncomon;
                }
            }
            else if (playerDB.ownedCards.Count < 60)
            {
                if (numRare345 > 1)
                {
                    return ETriadDeckState.TooManyRaresRare;
                }
            }
            else
            {
                if (numRare45 > 1)
                {
                    return ETriadDeckState.TooManyRaresEpic;
                }
            }

            return ETriadDeckState.Valid;
        }

        public TriadCard GetCard(int Idx)
        {
            if (Idx < knownCards.Count)
            {
                return knownCards[Idx];
            }
            else if (Idx < (knownCards.Count + unknownCardPool.Count))
            {
                return unknownCardPool[Idx - knownCards.Count];
            }

            return null;
        }

        public bool SetCard(int Idx, TriadCard card)
        {
            bool bResult = false;
            if (Idx < knownCards.Count)
            {
                knownCards[Idx] = card;
                bResult = true;
            }
            else if (Idx < (knownCards.Count + unknownCardPool.Count))
            {
                unknownCardPool[Idx - knownCards.Count] = card;
                bResult = true;
            }

            return bResult;
        }

        public int GetPower()
        {
            int SumRating = 0;
            foreach (TriadCard card in knownCards)
            {
                SumRating += (int)card.Rarity + 1;
            }

            foreach (TriadCard card in unknownCardPool)
            {
                SumRating += (int)card.Rarity + 1;
            }

            int NumCards = knownCards.Count + unknownCardPool.Count;
            int DeckPower = (NumCards > 0) ? System.Math.Min(System.Math.Max((SumRating * 2 / NumCards), 1), 10) : 1;

            return DeckPower;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TriadDeck);
        }

        public bool Equals(TriadDeck otherDeck)
        {
            if ((knownCards.Count != otherDeck.knownCards.Count) ||
                (unknownCardPool.Count != otherDeck.unknownCardPool.Count))
            {
                return false;
            }

            for (int Idx = 0; Idx < knownCards.Count; Idx++)
            {
                if (!knownCards[Idx].Equals(otherDeck.knownCards[Idx]))
                {
                    return false;
                }
            }

            for (int Idx = 0; Idx < unknownCardPool.Count; Idx++)
            {
                if (!unknownCardPool[Idx].Equals(otherDeck.unknownCardPool[Idx]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 739328532;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<TriadCard>>.Default.GetHashCode(knownCards);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<TriadCard>>.Default.GetHashCode(unknownCardPool);
            return hashCode;
        }

        public override string ToString()
        {
            string desc = "";
            foreach (TriadCard card in knownCards)
            {
                desc += card.ToShortString() + ", ";
            }

            desc = (desc.Length > 2) ? desc.Remove(desc.Length - 2, 2) : "(none)";

            if (unknownCardPool.Count > 0)
            {
                desc += " + unknown(";
                foreach (TriadCard card in unknownCardPool)
                {
                    desc += card.ToShortString() + ", ";
                }

                desc = desc.Remove(desc.Length - 2, 2);
                desc += ")";
            }

            int power = GetPower();
            desc += ", power:" + power;

            return desc;
        }
    }

    public abstract class TriadDeckInstance
    {
        public abstract void OnCardPlacedFast(int Idx);
        public abstract int GetFirstAvailableCardFast();
        public abstract TriadCard GetCard(int Idx);
        public abstract int GetCardIndex(TriadCard card);
        public abstract TriadDeckInstance CreateCopy();

        public TriadDeck deck;
        public int availableCardMask;
        public int numUnknownPlaced;
        public int numPlaced;

        // manual, player = 5
        // manual, npc = (up to 5 fixed) + (up to 5 variable)
        // screen, npc = (up to 5 hidden) + (up to 5 fixed) + (up to 5 variable)
        public const int maxAvailableCards = 15;

        public bool IsPlaced(int cardIdx)
        {
            return (availableCardMask & (1 << cardIdx)) == 0;
        }

        public TriadCard GetFirstAvailableCard()
        {
            int cardIdx = GetFirstAvailableCardFast();
            return (cardIdx < 0) ? null : GetCard(cardIdx);
        }

        public List<TriadCard> GetAvailableCards()
        {
            List<TriadCard> cards = new List<TriadCard>();
            for (int Idx = 0; Idx < maxAvailableCards; Idx++)
            {
                bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                if (bIsAvailable)
                {
                    TriadCard card = GetCard(Idx);
                    cards.Add(card);
                }
            }

            return cards;
        }
    }

    public class TriadDeckInstanceManual : TriadDeckInstance
    {
        public TriadDeckInstanceManual(TriadDeck deck)
        {
            this.deck = deck;
            numUnknownPlaced = 0;
            numPlaced = 0;
            availableCardMask = (1 << (deck.knownCards.Count + deck.unknownCardPool.Count)) - 1;
        }

        public TriadDeckInstanceManual(TriadDeckInstanceManual copyFrom)
        {
            deck = copyFrom.deck;
            numUnknownPlaced = copyFrom.numUnknownPlaced;
            numPlaced = copyFrom.numPlaced;
            availableCardMask = copyFrom.availableCardMask;
        }

        public override TriadDeckInstance CreateCopy()
        {
            TriadDeckInstanceManual deckCopy = new TriadDeckInstanceManual(this);
            return deckCopy;
        }

        public override void OnCardPlacedFast(int cardIdx)
        {
            availableCardMask &= ~(1 << cardIdx);
            numPlaced++;

            if (cardIdx >= deck.knownCards.Count)
            {
                const int maxCardsToPlace = ((TriadGameData.boardSize * TriadGameData.boardSize) / 2) + 1;
                int maxUnknownToPlace = maxCardsToPlace - deck.knownCards.Count;

                numUnknownPlaced++;
                if (numUnknownPlaced >= maxUnknownToPlace)
                {
                    availableCardMask &= ((1 << deck.knownCards.Count) - 1);
                }
            }
        }

        public override int GetFirstAvailableCardFast()
        {
            for (int Idx = 0; Idx < deck.knownCards.Count; Idx++)
            {
                bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                if (bIsAvailable)
                {
                    return Idx;
                }
            }

            return -1;
        }

        public override TriadCard GetCard(int Idx)
        {
            return deck.GetCard(Idx);
        }

        public override int GetCardIndex(TriadCard card)
        {
            int cardIdx = deck.knownCards.IndexOf(card);
            if (cardIdx < 0)
            {
                cardIdx = deck.unknownCardPool.IndexOf(card) + deck.knownCards.Count;
            }

            return cardIdx;
        }

        public override string ToString()
        {
            string desc = "Placed: " + numPlaced + ", Available: ";

            if (availableCardMask > 0)
            {
                for (int Idx = 0; Idx < maxAvailableCards; Idx++)
                {
                    bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                    if (bIsAvailable)
                    {
                        TriadCard card = GetCard(Idx);
                        desc += card.ToShortString() + ", ";
                    }
                }

                desc = desc.Remove(desc.Length - 2, 2);
            }
            else
            {
                desc += "none";
            }

            return desc;
        }
    }

    public class TriadDeckInstanceScreen : TriadDeckInstance
    {
        public TriadCard[] cards;
        public int unknownPoolMask;

        public TriadDeckInstanceScreen()
        {
            cards = new TriadCard[5];
            availableCardMask = 0;
            unknownPoolMask = 0;
            numUnknownPlaced = 0;
            numPlaced = 0;
        }

        public TriadDeckInstanceScreen(TriadDeckInstanceScreen copyFrom)
        {
            cards = new TriadCard[copyFrom.cards.Length];
            for (int Idx = 0; Idx < copyFrom.cards.Length; Idx++)
            {
                cards[Idx] = copyFrom.cards[Idx];
            }

            deck = copyFrom.deck;
            numUnknownPlaced = copyFrom.numUnknownPlaced;
            numPlaced = copyFrom.numPlaced;
            availableCardMask = copyFrom.availableCardMask;
            unknownPoolMask = copyFrom.unknownPoolMask;
        }

        public override TriadDeckInstance CreateCopy()
        {
            TriadDeckInstanceScreen deckCopy = new TriadDeckInstanceScreen(this);
            return deckCopy;
        }

        public override int GetFirstAvailableCardFast()
        {
            for (int Idx = 0; Idx < cards.Length; Idx++)
            {
                bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                if (bIsAvailable)
                {
                    return Idx;
                }
            }

            return -1;
        }

        public void UpdateAvailableCards(TriadCard[] screenCards)
        {
            availableCardMask = 0;
            numPlaced = 0;
            numUnknownPlaced = 0;
            cards = screenCards;

            int hiddenCardId = TriadCardDB.Get().hiddenCard.Id;
            for (int Idx = 0; Idx < cards.Length; Idx++)
            {
                if (cards[Idx] != null)
                {
                    if (cards[Idx].Id != hiddenCardId)
                    {
                        availableCardMask |= (1 << Idx);
                    }
                }
                else
                {
                    numPlaced++;
                }
            }
        }

        public override void OnCardPlacedFast(int cardIdx)
        {
            int cardMask = (1 << cardIdx);
            availableCardMask &= ~cardMask;

            if (deck != null)
            {
                bool bIsUnknown = (unknownPoolMask & cardMask) != 0;
                if (bIsUnknown)
                {
                    numUnknownPlaced++;

                    int maxUnknownToUse = cards.Length - deck.knownCards.Count;
                    if (numUnknownPlaced >= maxUnknownToUse)
                    {
                        availableCardMask &= ~unknownPoolMask;
                    }
                }
            }
        }

        public override TriadCard GetCard(int Idx)
        {
            return (Idx < 0) ? null :
                (Idx < cards.Length) ? cards[Idx] : 
                (deck != null) ? deck.GetCard(Idx - cards.Length) :
                null;
        }

        public override int GetCardIndex(TriadCard card)
        {
            int cardIdx = Array.IndexOf(cards, card);
            if (cardIdx < 0 && deck != null)
            {
                cardIdx = deck.knownCards.IndexOf(card);
                if (cardIdx < 0)
                {
                    cardIdx = deck.unknownCardPool.IndexOf(card);
                    if (cardIdx >= 0)
                    {
                        cardIdx += deck.knownCards.Count + cards.Length;
                    }
                }
                else
                {
                    cardIdx += cards.Length;
                }
            }

            return cardIdx;
        }

        public override string ToString()
        {
            string desc = "[SCREEN] Available: ";

            if (availableCardMask > 0)
            {
                for (int Idx = 0; Idx < cards.Length; Idx++)
                {
                    bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                    if (bIsAvailable)
                    {
                        TriadCard card = GetCard(Idx);
                        desc += (card != null ? card.Name : "") + ", ";
                    }
                }

                desc = desc.Remove(desc.Length - 2, 2);
            }
            else
            {
                desc += "none";
            }

            int visibleCardsMask = (cards != null) ? ((1 << cards.Length) - 1) : 0;
            bool hasHiddenCards = (availableCardMask & ~visibleCardsMask) != 0;
            if (hasHiddenCards)
            {
                desc += ", Unknown: ";
                if (deck != null)
                {
                    for (int Idx = cards.Length; Idx < maxAvailableCards; Idx++)
                    {
                        bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                        if (bIsAvailable)
                        {
                            TriadCard card = GetCard(Idx);
                            bool bIsKnownPool = (Idx - cards.Length) < deck.knownCards.Count;
                            desc += card.ToShortString() + ":" + Idx + ":" + (bIsKnownPool ? "K" : "U") + ", ";
                        }
                    }

                    desc = desc.Remove(desc.Length - 2, 2);
                }
                else
                {
                    desc += "(missing deck!)";
                }
            }

            return desc;
        }

        public void LogAvailableCards(string deckName)
        {
            Logger.WriteLine(deckName + " state> numPlaced:" + numPlaced + ", numUnknownPlaced:" + numUnknownPlaced);
            for (int Idx = 0; Idx < maxAvailableCards; Idx++)
            {
                bool bIsAvailable = (availableCardMask & (1 << Idx)) != 0;
                bool bIsUnknown = (unknownPoolMask & (1 << Idx)) != 0;
                TriadCard card = GetCard(Idx);

                Logger.WriteLine("   [" + Idx + "]:" + (card != null ? card.Name : "??") +
                    (bIsUnknown ? " (U)" : "") + " => " + (bIsAvailable ? "available" : "nope"));
            }
        }
    }
}
