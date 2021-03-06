﻿angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', ['$scope', 'BattleBitsService', '$location', '$interval', 'gameUrl', 'userId', function ($scope, BattleBitsService, $location, $interval, gameUrl, userId) {
        function setNextGameDelay(game) {
            var delay = Math.round((new Date(game.start) - new Date()) / 1000);
            $scope.timeTillNextGame = delay;
            $interval(function () {
                $scope.timeTillNextGame--;
            }, 1000, delay);
        }

        $scope.currentGame = BattleBitsService.currentGame;
        if ($scope.currentGame) {
            return $location.path('/viewer');
        }

        $scope.nextGame = BattleBitsService.nextGame;
        if ($scope.nextGame) {
            setNextGameDelay($scope.nextGame);
        }

        $scope.competition = BattleBitsService.competition;
        $scope.gameUrl = gameUrl;
        $scope.previousGameScores = BattleBitsService.previousGameScores;
        $scope.highScores = BattleBitsService.highScores;

        BattleBitsService.onCompetitionJoined($scope, function () {
            $scope.competition = BattleBitsService.competition;
            $scope.previousGameScores = BattleBitsService.previousGameScores;
            $scope.nextGame = BattleBitsService.nextGame;
            $scope.currentGame = BattleBitsService.currentGame;
            $scope.highScores = BattleBitsService.highScores;
        });

        BattleBitsService.onGameScheduled($scope, function () {
            $scope.nextGame = BattleBitsService.nextGame;
            setNextGameDelay($scope.nextGame);
        });

        $scope.hasJoined = function () {
            return $scope.nextGame
                && $scope.nextGame.scores
                && $scope.nextGame.scores[userId];
        };

        BattleBitsService.onGameStarted($scope, function () {
            if ($scope.hasJoined()) {
                return $location.path('/play');
            }
            return $location.path('/viewer');
        });

        BattleBitsService.onGameEnded($scope, function () {
            $scope.previousGameScores = BattleBitsService.previousGameScores;
            $scope.highScores = BattleBitsService.highScores;
        });

        $scope.playGame = function () {
            BattleBitsService.playGame();
        };
    }])
    .controller('GamePlayController', ['$scope', '$interval', 'BattleBitsService', '$location', function ($scope, $interval, BattleBitsService, $location) {
        if (BattleBitsService.currentGame == null) {
            return $location.path('/');
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
            return $location.path('/score');
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
            return $location.path('/');
        }

        $scope.timeLeft = $scope.game.duration;
        $interval(function () {
            $scope.timeLeft--;
        }, 1000, $scope.game.duration);

        $scope.scores = function(index) {
            var items = [];
            for (var key in $scope.game.scores) {
                if ($scope.game.scores.hasOwnProperty(key)) {
                    var score = $scope.game.scores[key];
                    if (score.score === (index + 1)) {
                        items.push(score);
                    }
                }
            }
            return items;
        };

        BattleBitsService.onGameEnded($scope, function () {
            return $location.path('/');
        });
    }])
    .service('BattleBitsService', ['competitionId', 'SignalR', '$rootScope', '$q', function(competitionId, SignalR, $rootScope, $q) {
        var BatteBitsService = function(competitionId, SignalR) {

            this.competition = null;
            this.nextGame = null;
            this.currentGame = null;
            this.previousGameScores = null;
            this.highScores = [];

            var that = this;
            var hub = SignalR.BattleBitsHub;
            hub.client.gameScheduled = function(event) {
                $rootScope.$apply(function() {
                    that.nextGame = event.game;
                    that.nextGame.delay = event.delay;
                    $rootScope.$emit('game-scheduled');
                });
            };

            hub.client.playerJoined = function(event) {
                $rootScope.$apply(function() {
                    if (that.nextGame) {
                        if (!that.nextGame.scores) {
                            that.nextGame.scores = [];
                        }
                        that.nextGame.scores[event.player.userId] = {
                            player: event.player,
                            time: 0,
                            score: 0
                        };
                        $rootScope.$emit('player-joined');
                    }
                });
            };

            hub.client.playerLeft = function(event) {
                $rootScope.$apply(function() {
                    if (that.nextGame
                        && that.nextGame.scores
                        && that.nextGame.scores[event.userId]) {
                            delete that.nextGame.scores[event.userId];
                    }
                    $rootScope.$emit('player-left');
                });
            };

            hub.client.gameStarted = function(event) {
                $rootScope.$apply(function() {
                    that.currentGame = event.game;
                    that.nextGame = null;
                    $rootScope.$emit('game-started');
                });
            };

            hub.client.gameEnded = function(event) {
                $rootScope.$apply(function() {
                    that.currentGame = null;
                    that.previousGameScores = event.previousGameScores;
                    that.highScores = event.highScores;
                    $rootScope.$emit('game-ended');
                });
            };

            hub.client.playerScored = function (event) {
                $rootScope.$apply(function () {
                    if (that.currentGame && that.currentGame.scores) {
                        that.currentGame.scores[event.score.player.userId] = event.score;
                    }
                    $rootScope.$emit('player-scored');
                });
            }

            this.playGame = function() {
                hub.server.joinGame(competitionId)
                    .fail(function(error) {
                        throw error; // TODO Handle Error
                    });
            };

            this.nextNumber = function(number, value) {
                var deferred = $q.defer();
                hub.server.nextNumber(competitionId, number, value)
                    .done(function(nextNumber) {
                        deferred.resolve();
                    })
                    .fail(function() {
                        deferred.reject();
                    });
                return deferred.promise;
            };

            this.onCompetitionJoined = function(scope, callback) {
                var unsubscribe = $rootScope.$on('competition-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameScheduled = function(scope, callback) {
                var unsubscribe = $rootScope.$on('game-scheduled', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameStarted = function(scope, callback) {
                var unsubscribe = $rootScope.$on('game-started', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerJoined = function(scope, callback) {
                var unsubscribe = $rootScope.$on('player-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerLeft = function(scope, callback) {
                var unsubscribe = $rootScope.$on('player-left', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameEnded = function(scope, callback) {
                var unsubscribe = $rootScope.$on('game-ended', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerScored = function (scope, callback) {
                var unsubscribe = $rootScope.$on('player-scored', callback);
                scope.$on('$destroy', unsubscribe);
            };

            // This MUST come at the end, or client callbacks wont be registered!
            SignalR.hub.start()
                .done(function() {
                    hub.server.joinCompetition(competitionId)
                        .done(function(session) {
                            $rootScope.$apply(function() {
                                that.competition = session.competition;
                                that.nextGame = session.nextGame;
                                that.currentGame = session.currentGame;
                                that.previousGameScores = session.previousGameScores;
                                that.highScores = session.highScores;
                                $rootScope.$emit('competition-joined');
                            });
                        }).fail(function(error) {
                            throw error; // TODO Handle error
                        });
                }).fail(function(error) {
                    throw error; // TODO Handle error
                });
            // Do not add code below this line
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