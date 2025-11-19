using NUnit.Framework;
using System.Collections.Generic;
using NeoPilot.Extension.CodeSuggestions.Model;

namespace NeoPilot.Extension.Tests.CodeSuggestions
{
    [TestFixture]
    public class SuggestionDisplaySessionTests
    {
        private SuggestionDisplaySession _session;

        [SetUp]
        public void Setup()
        {
            _session = new SuggestionDisplaySession();
        }

        [Test]
        public void New_ShouldResetSessionState()
        {
            // Arrange
            _session.Start(new List<Completion> { new Completion("1", new CompletionChunk("1", "test", false)) });

            // Act
            _session.New();

            // Assert
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Loading));
            Assert.That(_session.Completions, Is.Empty);
            Assert.That(_session.CurrentIndex, Is.EqualTo(0));
        }

        [Test]
        public void Start_ShouldInitializeSessionWithCompletions()
        {
            // Arrange
            var completions = new List<Completion> 
            { 
                new Completion("1", "test1"),
                new Completion("2", "test2")
            };

            // Act
            _session.Start(completions);

            // Assert
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Loading));
            Assert.That(_session.Completions, Has.Count.EqualTo(2));
            Assert.That(_session.CurrentIndex, Is.EqualTo(0));
            Assert.That(_session.CurrentCompletion.InsertText, Is.EqualTo("test1"));
        }

        [Test]
        public void Error_ShouldSetStateToError()
        {
            // Act
            _session.Error();

            // Assert
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Error));
        }

        [Test]
        public void Shown_ShouldSetStateToShown()
        {
            // Act
            _session.Shown();

            // Assert
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Shown));
        }

        [Test]
        public void AdditionalLoaded_ShouldAddNonDuplicateCompletions()
        {
            // Arrange
            _session.Start(new List<Completion> { new Completion("1", new CompletionChunk("test1", "test1", false)) });
            var additionalCompletions = new List<Completion>
            {
                new Completion("2", new CompletionChunk("test2", "test2", false)),
                new Completion("3", new CompletionChunk("test1", "test1", false)), // Duplicate, should not be added
                new Completion("3", new CompletionChunk(" test1 ", "test1", false)) // Duplicate with spaces, should not be added
            };

            // Act
            bool result = _session.AdditionalLoaded(additionalCompletions);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_session.Completions, Has.Count.EqualTo(2));
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Shown));
        }

        [Test]
        public void ChunkLoaded_ShouldUpdateExistingCompletion()
        {
            // Arrange
            _session.Start(new List<Completion> { new Completion("1", new CompletionChunk("hello", "stream-id-1", false)) });
            var chunk = new CompletionChunk("hello world", "stream-id-1", false);

            // Act
            bool result = _session.ChunkLoaded(chunk);

            // Assert
            Assert.That(result, Is.True);
            Assert.AreEqual(SuggestionState.Loading, _session.State);
            Assert.That(_session.CurrentCompletion.InsertText, Is.EqualTo("hello world"));
        }
        
        [Test]
        public void LastChunkLoaded_ShouldTransitToShown()
        {
            // Arrange
            _session.Start(new List<Completion> { new Completion("1", new CompletionChunk("hello", "stream-id-1", false)) });
            var chunk = new CompletionChunk("hello world", "stream-id-1", true);

            // Act
            _session.ChunkLoaded(chunk);

            // Assert
            Assert.AreEqual(SuggestionState.Shown, _session.State);
        }
        
        [Test]
        public void LastChunkLoaded_ShouldNotTransitToShown_WithMismatchingId()
        {
            // Arrange
            _session.Start(new List<Completion> { new Completion("1", new CompletionChunk("hello", "stream-id-1", false)) });
            var chunk = new CompletionChunk("hello world", "stream-id-2", true);

            // Act
            _session.ChunkLoaded(chunk);

            // Assert
            Assert.AreEqual(SuggestionState.Loading, _session.State);
        }

        [Test]
        public void Next_ShouldMoveToNextCompletion()
        {
            // Arrange
            var completions = new List<Completion> 
            { 
                new Completion("1", "test1"),
                new Completion("2", "test2")
            };
            _session.Start(completions);

            // Act
            _session.Next();

            // Assert
            Assert.That(_session.CurrentIndex, Is.EqualTo(1));
            Assert.That(_session.CurrentCompletion.InsertText, Is.EqualTo("test2"));
        }

        [Test]
        public void Previous_ShouldMoveToPreviousCompletion()
        {
            // Arrange
            var completions = new List<Completion> 
            { 
                new Completion("1", "test1"),
                new Completion("2", "test2")
            };
            _session.Start(completions);
            _session.Next(); // Move to the second completion

            // Act
            _session.Previous();

            // Assert
            Assert.That(_session.CurrentIndex, Is.EqualTo(0));
            Assert.That(_session.CurrentCompletion.InsertText, Is.EqualTo("test1"));
        }

        [Test]
        public void Complete_ShouldSetStateToComplete()
        {
            // Act
            _session.Complete();

            // Assert
            Assert.That(_session.State, Is.EqualTo(SuggestionState.Complete));
        }

        [Test]
        public void GetShownCompletions_ShouldReturnOnlyShownCompletions()
        {
            // Arrange
            var completions = new List<Completion> 
            { 
                new Completion("1", "test1"),
                new Completion("2", "test2"),
                new Completion("3", "test3")
            };
            _session.Start(completions);
            _session.Next(); // Show the second completion

            // Act
            var shownCompletions = _session.GetShownCompletions();

            // Assert
            Assert.That(shownCompletions, Has.Count.EqualTo(2));
            Assert.That(shownCompletions[0].InsertText, Is.EqualTo("test1"));
            Assert.That(shownCompletions[1].InsertText, Is.EqualTo("test2"));
        }

        [Test]
        public void GetNotShownCompletions_ShouldReturnOnlyNotShownCompletions()
        {
            // Arrange
            var completions = new List<Completion> 
            { 
                new Completion("1", "test1"),
                new Completion("2", "test2"),
                new Completion("3", "test3")
            };
            _session.Start(completions);
            _session.Next(); // Show the second completion

            // Act
            var notShownCompletions = _session.GetNotShownCompletions();

            // Assert
            Assert.That(notShownCompletions, Has.Count.EqualTo(1));
            Assert.That(notShownCompletions[0].UniqueTrackingId, Is.EqualTo("3"));
        }
    }
}
