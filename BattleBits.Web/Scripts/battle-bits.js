angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', ['$scope', 'BattleBitsService', '$location', '$interval', 'gameUrl', 'userId', function ($scope, BattleBitsService, $location, $interval, gameUrl, userId) {
        if ($scope.currentGame) {
            $location.path('/viewer');
        }
        
        $scope.nextGame = BattleBitsService.nextGame;
        $scope.previousGame = BattleBitsService.previousGame;
        $scope.gameUrl = gameUrl;
        $scope.competition = BattleBitsService.competition;
        $scope.highScores = BattleBitsService.highScores;

        BattleBitsService.onCompetitionJoined($scope, function () {
            $scope.competition = BattleBitsService.competition;
            $scope.previousGame = BattleBitsService.previousGame;
            $scope.nextGame = BattleBitsService.nextGame;
            $scope.currentGame = BattleBitsService.currentGame;
            $scope.highScores = BattleBitsService.highScores;
        });

        BattleBitsService.onGameScheduled($scope, function () {
            $scope.nextGame = BattleBitsService.nextGame;
            $scope.timeTillNextGame = $scope.nextGame.delay;
            $interval(function () {
                $scope.timeTillNextGame--;
            }, 1000, $scope.nextGame.delay);
        });

        $scope.hasJoined = function() {
            if ($scope.nextGame && $scope.nextGame.scores) {
                for (var i = 0; i < $scope.nextGame.scores.length; i++) {
                    if ($scope.nextGame.scores[i].player
                        && $scope.nextGame.scores[i].player.userId === userId) {
                        return true;
                    }
                }
            }
            return false;
        };

        BattleBitsService.onGameStarted($scope, function () {
            if ($scope.hasJoined()) {
                $location.path('/play');
            } else {
                $location.path('/viewer');
            }
        });

        BattleBitsService.onGameEnded($scope, function () {
            $scope.previousGame = BattleBitsService.previousGame;
        });

        $scope.playGame = function () {
            BattleBitsService.playGame();
            $scope.enlisted = true;
        };
    }])
    .controller('GamePlayController', ['$scope', '$interval', 'BattleBitsService', '$location', function ($scope, $interval, BattleBitsService, $location) {
        if (BattleBitsService.currentGame == null) {
            $location.path('/');
        }
        var currentGame = BattleBitsService.currentGame;

        $scope.guess = 0;
        $scope.numbersGuessed = 0;
        $scope.numbers = currentGame.numbers;
        $scope.numbersToGuess = $scope.numbers.length;
        $scope.timeLeft = currentGame.duration;
        $scope.number = $scope.numbers[$scope.numbersGuessed];

        $scope.isBitActive = function(bitPosition) {
            return ($scope.guess & (1 << bitPosition)) !== 0;
        };

        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };

        var timer = $interval(function() {
            $scope.timeLeft--;
        }, 1000, currentGame.duration);

        BattleBitsService.onGameEnded($scope, function () {
            $location.path('/score');
        });

        var numberWatcher = $scope.$watch('guess', function(newValue) {
            if (newValue === $scope.number) {
                BattleBitsService.nextNumber($scope.numbersGuessed, newValue)
                    .then(function() {
                        $scope.numbersGuessed++;
                        if ($scope.numbersGuessed === $scope.numbersToGuess) {
                            $interval.cancel(timer);
                            // TODO $location.path('/score/' + time + ) --> show score
                        } else {
                            $scope.number = $scope.numbers[$scope.numbersGuessed];
                            $scope.guess = 0;
                        }
                    }, function() {
                        // Try again, wrong guess
                    });
            }
        });
    }])
    .controller('GameDisplayController', ['$scope', 'BattleBitsService', '$location', '$interval', function ($scope, BattleBitsService, $location, $interval) {
        $scope.competition = BattleBitsService.competition;
        $scope.game = BattleBitsService.currentGame;

        if ($scope.game == null) {
            $location.path('/');
        }

        $scope.timeLeft = $scope.game.duration;
        $interval(function () {
            $scope.timeLeft--;
        }, 1000, $scope.game.duration);

        BattleBitsService.onGameEnded($scope, function () {
            $location.path('/');
        });
    }])
    .service('BattleBitsService', ['competitionId', 'SignalR', '$rootScope', '$q', function(competitionId, SignalR, $rootScope, $q) {
        var BatteBitsService = function (competitionId, SignalR) {

            this.competition = null;
            this.nextGame = null;
            this.currentGame = null;
            this.previousGame = null;
            this.highScores = [];

            var that = this;
            var hub = SignalR.BattleBitsHub;
            SignalR.hub.start()
                .done(function() {
                    hub.server.joinCompetition(competitionId)
                        .done(function(session) {
                            $rootScope.$apply(function() {
                                that.competition = session.competition;
                                that.nextGame = session.nextGame;
                                that.currentGame = session.currentGame;
                                that.previousGame = session.previousGame;
                                that.highScores = session.highScores;
                                $rootScope.$emit('competition-joined');
                            });
                        }).fail(function(error) {
                            throw error; // TODO Handle error
                        });
                }).fail(function(error) {
                    throw error; // TODO Handle error
                });

            hub.client.gameScheduled = function(event) {
                $rootScope.$apply(function() {
                    that.nextGame = event.game;
                    that.nextGame.delay = event.delay;
                    $rootScope.$emit('game-scheduled');
                });
            };

            hub.client.playerJoined = function (event) {
                $rootScope.$apply(function () {
                    if (that.nextGame) {
                        if (!that.nextGame.scores) {
                            that.nextGame.scores = [];
                        }
                        that.nextGame.scores.push({
                            player: event.player,
                            time: 0,
                            score: 0
                        });
                        $rootScope.$emit('player-joined');
                    }
                });
            };

            hub.client.playerLeft = function (event) {
                $rootScope.$apply(function () {
                    $rootScope.$emit('player-left');
                });
            };

            hub.client.gameStarted = function (event) {
                $rootScope.$apply(function() {
                    that.currentGame = event.game;
                    that.nextGame = null;
                    $rootScope.$emit('game-started');
                });
            };

            hub.client.gameEnded = function (event) {
                $rootScope.$apply(function () {
                    that.currentGame = null;
                    that.previousGame = event.game;
                    $rootScope.$emit('game-ended');
                });
            };

            this.playGame = function() {
                hub.server.joinGame(competitionId)
                    .fail(function(error) {
                        throw error;  // TODO Handle Error
                    });
            };

            this.nextNumber = function (number, value) {
                var deferred = $q.defer();
                hub.server.nextNumber(competitionId, number, value)
                    .done(function (nextNumber) {
                        deferred.resolve();
                    })
                    .fail(function () {
                        deferred.reject();
                    });
                return deferred.promise;
            };

            this.onCompetitionJoined = function (scope, callback) {
                var unsubscribe = $rootScope.$on('competition-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameScheduled = function(scope, callback) {
                var unsubscribe = $rootScope.$on('game-scheduled', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameStarted = function (scope, callback) {
                var unsubscribe = $rootScope.$on('game-started', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerJoined = function (scope, callback) {
                var unsubscribe = $rootScope.$on('player-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerLeft = function (scope, callback) {
                var unsubscribe = $rootScope.$on('player-left', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameEnded = function (scope, callback) {
                var unsubscribe = $rootScope.$on('game-ended', callback);
                scope.$on('$destroy', unsubscribe);
            };
        };
        return new BatteBitsService(competitionId, SignalR);
    }])
    .filter('binary', function () {
        return function(input, length) {
            length = length || 8;
            return _.padStart((input >>> 0).toString(2), length, '0');
        };
    })
    .filter('hex', function() {
        return function(input) {
            return "0x" + input.toString(16);
        };
    });