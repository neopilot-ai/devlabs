using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NeoPilot.Extension.CodeSuggestions;
using NeoPilot.Extension.CodeSuggestions.Model;
using NeoPilot.Extension.LanguageServer;
using NeoPilot.Extension.Workspace;
using Moq;
using NUnit.Framework;
using Serilog;

namespace NeoPilot.Extension.Tests.CodeSuggestions
{
    [TestFixture]
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
    public class NeoPilotCodeSuggestionTelemetryControllerTests
    {
        private const string TestTelemetryId = "test-telemetry-id";
        private const int OptionId = 2;
        
        private Mock<ILsClientProvider> _mockLsClientProvider;
        private Mock<ILsClient> _mockLsClient;
        private Mock<ILogger> _mockLogger;
        private NeoPilotCodeSuggestionTelemetryController _controller;

        private static readonly WorkspaceId _testWorkspaceId =
            new WorkspaceId(
                "TestSolution",
                "path/to/solution");
        
        private static readonly Completion _completion = new Completion(TestTelemetryId, "Hello world", "stream-id-1", OptionId);
        
        [SetUp]
        public void SetUp()
        {
            _mockLsClientProvider = new Mock<ILsClientProvider>();
            _mockLsClient = new Mock<ILsClient>();
            _mockLogger = new Mock<ILogger>();
            _controller = new NeoPilotCodeSuggestionTelemetryController(_mockLsClientProvider.Object, _mockLogger.Object);
            
            _mockLsClientProvider.Setup(p => p.GetClientAsync()).ReturnsAsync(_mockLsClient.Object);
        }
        
        [Test]
        public async Task OnShownAsync_ShouldSendShownTelemetry()
        {
            // Act
            await _controller.OnShownAsync(_completion.UniqueTrackingId);

            // Assert
            _mockLsClient.Verify(client => client.SendNeopilotTelemetryCodeSuggestionShownAsync(TestTelemetryId), Times.Once);
            _mockLogger.VerifyNoOtherCalls();
        }
        
        [Test]
        public async Task OnRejectedAsync_AfterOnShownAsync_ShouldSendRejectedTelemetry()
        {
            // Arrange
            await _controller.OnShownAsync(_completion.UniqueTrackingId);
            
            // Act
            await _controller.OnRejectedAsync(_completion.UniqueTrackingId);

            // Assert
            _mockLsClient.Verify(client => client.SendNeopilotTelemetryCodeSuggestionRejectedAsync(TestTelemetryId), Times.Once);
            _mockLogger.VerifyNoOtherCalls();
        }
        
        [Test]
        public async Task OnAcceptedAsync_AfterOnShownAsync_ShouldSendAcceptedTelemetry()
        {
            // Arrange
            await _controller.OnShownAsync(_completion.UniqueTrackingId);
            
            // Act
            await _controller.OnAcceptedAsync(_completion.UniqueTrackingId, _completion.OptionId);

            // Assert
            _mockLsClient.Verify(client => client.SendNeopilotTelemetryCodeSuggestionAcceptedAsync(TestTelemetryId, OptionId), Times.Once);
            _mockLogger.VerifyNoOtherCalls();
        }
        
        [Test]
        public async Task OnShownAsync_WhenExceptionOccurs_ShouldLogError()
        {
            // Arrange
            _mockLsClient.Setup(client => client.SendNeopilotTelemetryCodeSuggestionShownAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));
            
            // Act
            await _controller.OnShownAsync(_completion.UniqueTrackingId);

            // Assert
            _mockLogger.Verify(logger => logger.Error(It.IsAny<Exception>(), It.IsAny<string>(), TestTelemetryId), Times.Once);
        }
        
        [Test]
        public async Task OnShownAndAcceptedAsync_ForSameProposal_ShouldProcessBoth()
        {
            // Act
            await _controller.OnShownAsync(_completion.UniqueTrackingId);
            await _controller.OnAcceptedAsync(_completion.UniqueTrackingId, _completion.OptionId);

            // Assert
            _mockLsClient.Verify(client => client.SendNeopilotTelemetryCodeSuggestionShownAsync(TestTelemetryId), Times.Once);
            _mockLsClient.Verify(client => client.SendNeopilotTelemetryCodeSuggestionAcceptedAsync(TestTelemetryId, OptionId), Times.Once);
        }
    }
}
